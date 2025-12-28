using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Common.Logging;
using Grpc.Core;
using Makaretu.Dns;

namespace Warpinator
{
    class Server
    {
        readonly ILog log = Program.Log.GetLogger("Server");
        const string SERVICE_TYPE = "_warpinator._tcp";
        readonly DomainName ServiceDomain = new DomainName(SERVICE_TYPE+".local");

        public static Server current;

        public string DisplayName;
        public string UserName;
        public string Hostname;
        public ushort Port = 42000;
        public ushort AuthPort = 42001;
        public string UUID;
        public bool Running = false;
        public string SelectedInterface;
        public IPAddress SelectedIP { get; private set; }

        public Dictionary<string, Remote> Remotes = new Dictionary<string, Remote>();

        Grpc.Core.Server grpcServer;
        Grpc.Core.Server regServer;
        ServiceDiscovery sd;
        readonly MulticastService mdns;
        ServiceProfile serviceProfile;
        readonly ConcurrentDictionary<string, ServiceRecord> mdnsServices = new ConcurrentDictionary<string, ServiceRecord>();
        readonly ConcurrentDictionary<string, IPAddress> hostnameDict = new ConcurrentDictionary<string, IPAddress>();
        Properties.Settings settings;
        Timer pingTimer = new Timer(10_000);
        List<NetworkInterface> knownNics = null;
        private readonly Regex hostRegex = new Regex("(warpinator://)?(\\d{1,3}(\\.\\d{1,3}){3}):(\\d{1,6})/?$");
        private uint APIVersion = 2;
        bool restarting = false;
        bool needsRestart = false; //Needs another restart - in case restart was initiated while previous was in progress

        public Server()
        {
            current = this;
            try {
                DisplayName = System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName ?? Environment.UserName;
            } catch { // Mono does not have directory services
                DisplayName = Environment.UserName;
            }
            Hostname = Environment.MachineName;
            UserName = Environment.UserName;
            
            //Load settings
            settings = Properties.Settings.Default;
            bool needsSave = false;
            if (!String.IsNullOrEmpty(settings.UUID))
                UUID = settings.UUID;
            else
            {
                UUID = Hostname.ToUpper() + "-" + String.Format("{0:X6}", new Random().Next(0x1000000));
                settings.UUID = UUID;
                needsSave = true;
            }
            if (String.IsNullOrEmpty(settings.DownloadDir))
            {
                settings.DownloadDir = Path.Combine(Utils.GetDefaultDownloadFolder(), "Warpinator");
                Directory.CreateDirectory(settings.DownloadDir);
                needsSave = true;
            }
            if (needsSave)
                settings.Save();
            SelectedInterface = settings.NetworkInterface;
            SelectedIP = Utils.GetLocalIPAddress();
            mdns = new MulticastService((ifaces) => ifaces.Where((iface) => SelectedInterface == null || iface.Id == SelectedInterface));
            mdns.UseIpv6 = false;
            mdns.IgnoreDuplicateMessages = true;
            pingTimer.Elapsed += (a, b) => PingRemotes();
            pingTimer.AutoReset = true;
        }

        public async Task Start()
        {
            log.Info("-- Starting server");
            Running = true;
            Authenticator.GroupCode = settings.GroupCode;
            Port = settings.Port;
            AuthPort = settings.AuthPort;
            if (String.IsNullOrEmpty(settings.NetworkInterface))
                SelectedInterface = Utils.AutoSelectNetworkInterface();
            else SelectedInterface = settings.NetworkInterface;
            SelectedIP = Utils.GetLocalIPAddress();
            await StartGrpcServer(); //Also initializes authenticator for certserver
            CertServer.Start(Port);
            StartMDNS();
            pingTimer.Start();
            Form1.UpdateLabels();
        }

        public async Task Stop()
        {
            if (!Running)
                return;
            Running = false;
            pingTimer.Stop();
            sd.Unadvertise(serviceProfile);
            mdns.Stop();
            NetworkChange.NetworkAddressChanged -= OnNetworkChanged;
            CertServer.Stop();
            await grpcServer.KillAsync();
            await regServer.KillAsync();
            Form1.UpdateLabels();
            log.Info("-- Server stopped\n");
        }

        public async void Restart()
        {
            if (restarting)
            {
                log.Debug("- Restart in progress, will restart again after it finishes...");
                needsRestart = true;
                return;
            }
            restarting = true;
            log.Info(">> Restarting server");
            needsRestart = false;
            await Stop();
            await Start();
            restarting = false;
            if (needsRestart)
            {
                log.Debug("- Executing deferred restart");
                Restart();
            }
        }

        public void Rescan() => sd.QueryServiceInstances(SERVICE_TYPE);
        public void Reannounce() => sd.Announce(serviceProfile);

        private async Task StartGrpcServer()
        {
            KeyCertificatePair kcp = await Task.Run(Authenticator.GetKeyCertificatePair);
            SelectedIP = Utils.GetLocalIPAddress();
            var options = new ChannelOption[] { // As per Linux Warpinator source
                new ChannelOption("grpc.keepalive_time_ms", 10 * 1000),
                new ChannelOption("grpc.keepalive_timeout_ms", 5 * 1000),
                new ChannelOption("grpc.keepalive_permit_without_calls", 1),
                new ChannelOption("grpc.http2.max_pings_without_data", 0),
                new ChannelOption("grpc.http2.min_time_between_pings_ms", 10 * 1000),
                new ChannelOption("grpc.http2.min_ping_interval_without_data_ms",  5 * 1000)
            };
            grpcServer = new Grpc.Core.Server(options) {
                Services = { Warp.BindService(new GrpcService()) },
                Ports = { new ServerPort(SelectedIP.ToString(), Port, new SslServerCredentials(new List<KeyCertificatePair>() { kcp })) }
            };
            grpcServer.Start();
            try {
                regServer = new Grpc.Core.Server() {
                    Services = { WarpRegistration.BindService(new RegistrationService()) },
                    Ports = { new ServerPort(SelectedIP.ToString(), AuthPort, ServerCredentials.Insecure) }
                };
                regServer.Start();
            } catch (Exception e) {
                APIVersion = 1; // Fall back
                log.Warn("Failed to start V2 registration service, only V1 will be available", e);
            }
            log.Info("GRPC started");
        }

        private void StartMDNS(bool flush = false)
        {
            log.Debug("Starting mdns");
            if (knownNics == null)
                knownNics = MulticastService.GetNetworkInterfaces().ToList();
            NetworkChange.NetworkAddressChanged += OnNetworkChanged;
            sd = new ServiceDiscovery(mdns);
            sd.ServiceInstanceDiscovered += OnServiceInstanceDiscovered;
            sd.ServiceInstanceShutdown += OnServiceInstanceShutdown;
            mdns.AnswerReceived += OnAnswerReceived;

            mdns.Start();
            sd.QueryServiceInstances(SERVICE_TYPE);

            serviceProfile = new ServiceProfile(UUID, SERVICE_TYPE, Port, new List<IPAddress> { SelectedIP });
            // Only ASCII allowed, will replace other chars with '?'
            serviceProfile.AddProperty("hostname", Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(Utils.GetHostname())));
            serviceProfile.AddProperty("type", flush ? "flush" : "real");
            serviceProfile.AddProperty("api-version", APIVersion.ToString());
            serviceProfile.AddProperty("auth-port", AuthPort.ToString());
            sd.Advertise(serviceProfile);
            sd.Announce(serviceProfile);
        }

        private void PingRemotes()
        {
            foreach (var r in Remotes.Values)
            {
                if (r.Status == RemoteStatus.CONNECTED)
                    if (r.APIVersion == 1)
                        r.Ping();
                    else
                        r.CheckChannelState();

            }
        }

        public async Task<string> RegisterWithHost(string host)
        {
            log.Info($"Registering with host {host}");
            Uri uri;
            try {
                uri = new Uri(host);
            } catch (UriFormatException) {
                return Resources.Strings.invalid_address;
            }
            string ip = uri.Host;
            int authport = uri.Port;
            try
            {
                var regChannel = new Channel(ip, authport, ChannelCredentials.Insecure);
                var regClient = new WarpRegistration.WarpRegistrationClient(regChannel);
                var resp = await regClient.RegisterServiceAsync(GetServiceRegistrationMsg(), deadline: DateTime.UtcNow.AddSeconds(10));
            
                Remote r;
                bool newRemote = !Remotes.TryGetValue(resp.ServiceId, out r);
                if (newRemote)
                {
                    r = new Remote();
                    r.UUID = resp.ServiceId;
                }
                else if (r.Status == RemoteStatus.CONNECTED)
                {
                    log.Warn("Remote already connected");
                    return Resources.Strings.already_connected;
                }
                r.UpdateFromServiceRegistration(resp);
                // Keep what the user specified
                r.Address = IPAddress.Parse(ip);
                r.AuthPort = authport;
                if (newRemote)
                    AddRemote(r);
                else
                {
                    if (r.Status == RemoteStatus.DISCONNECTED || r.Status == RemoteStatus.ERROR)
                        r.Connect();
                    else
                        r.UpdateUI();
                }
            }
            catch (RpcException e)
            {
                log.Error($"Manual connection failed with RpcException: {e.StatusCode} - {e.Status.Detail}");
                if (e.StatusCode == StatusCode.Unimplemented)
                    return Resources.Strings.manual_connect_unsupported;
                else if (e.StatusCode == StatusCode.Unavailable || e.StatusCode == StatusCode.DeadlineExceeded)
                    return Resources.Strings.manual_connect_unavailable;
                else
                    return String.Format(Resources.Strings.manual_connect_error, e.StatusCode);
            }
            catch (Exception e) //Are other exceptions exepcted?
            {
                log.Error("Manual connect failed", e);
                return String.Format(Resources.Strings.manual_connect_error, e.Message);
            }
            return null;
        }

        public ServiceRegistration GetServiceRegistrationMsg()
        {
            return new ServiceRegistration {
                ServiceId = UUID,
                Ip = SelectedIP.ToString(),
                Port = Port,
                AuthPort = AuthPort,
                Hostname = Hostname,
                ApiVersion = APIVersion
            };
        }

        private void OnNetworkChanged(object s, EventArgs a)
        {
            var nics = MulticastService.GetNetworkInterfaces();
            var oldNics = knownNics.Where(k => !nics.Any(n => k.Id == n.Id));
            oldNics.ToList().ForEach((n) => log.Debug("-- Removed iface: " + n.Name));
            var newNics = nics.Where(nic => !knownNics.Any(k => k.Id == nic.Id));
            newNics.ToList().ForEach((n) => log.Debug("++ Added iface: " + n.Name));
            if (newNics.Any() || oldNics.Any())
            {
                knownNics = nics.ToList();
                string newBestNIC = String.IsNullOrEmpty(settings.NetworkInterface) ? Utils.AutoSelectNetworkInterface(true) : settings.NetworkInterface;
                var newAddress = Utils.GetIPAddressForNIC(newBestNIC);
                if (!SelectedIP.Equals(newAddress) && !newAddress.Equals(IPAddress.Loopback))
                {
                    log.Info($"New address: {SelectedIP} -> {newAddress}");
                    SelectedInterface = null;
                    SelectedIP = newAddress;
                    Restart();
                    return;
                }
            }
            NetworkChange.NetworkAddressChanged -= OnNetworkChanged;
            NetworkChange.NetworkAddressChanged += OnNetworkChanged;
        }

        private void OnServiceInstanceDiscovered(object sender, ServiceInstanceDiscoveryEventArgs e)
        {
            var srvName = String.Join(".", e.ServiceInstanceName.Labels);
            log.Debug($"Service discovered: '{srvName}'");
            if (!mdnsServices.ContainsKey(e.ServiceInstanceName.ToCanonical().ToString()))
                mdnsServices.TryAdd(e.ServiceInstanceName.ToCanonical().ToString(), new ServiceRecord() { FullName = srvName });
            else
            {
                mdnsServices.TryGetValue(e.ServiceInstanceName.ToCanonical().ToString(), out ServiceRecord rec);
                rec.resolved = false;
            }
        }
        
        private void OnServiceInstanceShutdown(object sender, ServiceInstanceShutdownEventArgs e)
        {
            log.Debug($"Service lost: '{String.Join(".", e.ServiceInstanceName.Labels)}'");
            string serviceId = e.ServiceInstanceName.Labels[0];
            if (Remotes.ContainsKey(serviceId))
            {
                var r = Remotes[serviceId];
                r.ServiceAvailable = false;
                r.UpdateUI();
            }
        }

        private void OnAnswerReceived(object sender, MessageEventArgs e)
        {
            //log.Debug($"-- Answer {e.Message.Id}:");
            var answers = e.Message.Answers.Concat(e.Message.AdditionalRecords).Where((r)=>r.Name.IsSubdomainOf(ServiceDomain) || r is AddressRecord);

            var servers = answers.OfType<SRVRecord>();
            foreach (var server in servers)
            {
                if (server.TTL == TimeSpan.Zero)
                    continue;
                var srvName = String.Join(".", server.Name.Labels);
                //log.Debug($"  Service '{srvName}' has hostname '{server.Target} and port {server.Port}'");
                if (!mdnsServices.ContainsKey(server.CanonicalName))
                    mdnsServices.TryAdd(server.CanonicalName, new ServiceRecord { FullName = srvName });
                mdnsServices[server.CanonicalName].Hostname = server.Target.ToString();
                if (hostnameDict.TryGetValue(server.Target.ToString(), out IPAddress addr))
                    mdnsServices[server.CanonicalName].Address = addr;
                mdnsServices[server.CanonicalName].Port = server.Port;
            }

            var addresses = answers.OfType<AddressRecord>();
            foreach (var address in addresses)
            {
                if (address.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // is IPv4
                {
                    log.Debug($"  Hostname '{address.Name}' resolves to {address.Address}");
                    hostnameDict.AddOrUpdate(address.Name.ToString(), address.Address, (a, b) => address.Address);
                    var svc = mdnsServices.Values.Where((s) => (s.Hostname == address.Name.ToString())).FirstOrDefault();
                    if (svc != null)
                        svc.Address = address.Address;
                }
            }

            var txts = answers.OfType<TXTRecord>();
            foreach (var txt in txts)
            {
                if (txt.TTL == TimeSpan.Zero)
                    continue;
                log.Debug("  Got strings: " + String.Join("; ", txt.Strings));
                mdnsServices[txt.CanonicalName].Txt = txt.Strings;
            }

            foreach (var svc in mdnsServices.Values)
            {
                if (!svc.resolved && svc.Address != null && svc.Txt != null)
                    OnServiceResolved(svc);
            }
        }

        private void OnServiceResolved(ServiceRecord svc)
        {
            string name = svc.FullName.Split('.')[0];
            log.Debug("Resolved " + name);
            if (name == UUID)
            {
                log.Debug("That's me - ignoring...");
                svc.resolved = true;
                return;
            }

            var txt = new Dictionary<string, string>();
            svc.Txt.ForEach((t) => { var s = t.Split('='); txt.Add(s[0], s[1]); });
            // Ignore flush registration
            if (txt.ContainsKey("type") && txt["type"] == "flush")
            {
                log.Trace("Ignoring flush registration");
                return;
            }

            svc.resolved = true; //TODO: support svc being updated
            lock (Remotes)
            {
                if (Remotes.ContainsKey(name))
                {
                    Remote r = Remotes[name];
                    log.Debug($"Service already known, status: {r.Status}");
                    if (txt.ContainsKey("hostname"))
                        r.Hostname = txt["hostname"];
                    r.ServiceAvailable = true;
                    if (r.Status == RemoteStatus.DISCONNECTED || r.Status == RemoteStatus.ERROR)
                    {
                        //TODO: Update and reconnect
                    }
                    else r.UpdateUI();
                    return;
                }

                Remote remote = new Remote();
                remote.Address = svc.Address;
                if (txt.ContainsKey("hostname"))
                    remote.Hostname = txt["hostname"];
                remote.Port = svc.Port;
                if (txt.ContainsKey("api-version"))
                    if (!uint.TryParse(txt["api-version"], out remote.APIVersion))
                        log.Warn("Invalid API version in TXT record");
                if (txt.ContainsKey("auth-port"))
                    if (!int.TryParse(txt["auth-port"], out remote.AuthPort))
                        log.Warn("Invalid auth port in TXT record");
                remote.ServiceName = name;
                remote.UUID = name;
                remote.ServiceAvailable = true;

                AddRemote(remote);
            }
        }

        public void AddRemote(Remote remote)
        {
            lock (Remotes)
            {
                if (!Remotes.ContainsKey(remote.UUID))
                    Remotes.Add(remote.UUID, remote);
            }
            Form1.UpdateUI();
            remote.Connect();
        }

        private class ServiceRecord
        {
            public string FullName;
            public string Hostname;
            public IPAddress Address;
            public int Port;
            public List<string> Txt;
            public bool resolved = false;
        }
    }
}
