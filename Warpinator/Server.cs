using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        public string UUID;
        public bool Running = false;
        public string SelectedInterface;
        public IPAddress SelectedIP { get; private set; }

        public Dictionary<string, Remote> Remotes = new Dictionary<string, Remote>();

        Grpc.Core.Server grpcServer;
        ServiceDiscovery sd;
        readonly MulticastService mdns;
        ServiceProfile serviceProfile;
        readonly ConcurrentDictionary<string, ServiceRecord> mdnsServices = new ConcurrentDictionary<string, ServiceRecord>();
        readonly ConcurrentDictionary<string, IPAddress> hostnameDict = new ConcurrentDictionary<string, IPAddress>();
        internal Properties.Settings settings = Properties.Settings.Default;
        Timer pingTimer = new Timer(10_000);

        public Server()
        {
            current = this;
            DisplayName = System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName ?? Environment.UserName;
            Hostname = Environment.MachineName;
            UserName = Environment.UserName;
            
            //Load settings
            settings = (Properties.Settings)System.Configuration.SettingsBase.Synchronized(Properties.Settings.Default);
            if (!String.IsNullOrEmpty(settings.UUID))
                UUID = settings.UUID;
            else
            {
                UUID = Hostname.ToUpper() + "-" + String.Format("{0:X6}", new Random().Next(0x1000000));
                settings.UUID = UUID;
            }
            if (String.IsNullOrEmpty(settings.DownloadDir))
            {
                settings.DownloadDir = Path.Combine(Utils.GetDefaultDownloadFolder(), "Warpinator");
                Directory.CreateDirectory(settings.DownloadDir);
            }
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
            if (String.IsNullOrEmpty(settings.NetworkInterface))
                SelectedInterface = Utils.AutoSelectNetworkInterface();
            else SelectedInterface = settings.NetworkInterface;
            await StartGrpcServer(); //Also initializes authenticator for certserver
            CertServer.Start(Port);
            StartMDNS();
            pingTimer.Start();
            Form1.UpdateUI();
        }

        public async Task Stop()
        {
            if (!Running)
                return;
            Running = false;
            pingTimer.Stop();
            sd.Unadvertise(serviceProfile);
            mdns.Stop();
            CertServer.Stop();
            await grpcServer.ShutdownAsync();
            Form1.UpdateUI();
            log.Info("-- Server stopped");
        }

        public async void Restart()
        {
            await Stop();
            await Start();
        }

        public void Rescan() => sd.QueryServiceInstances(SERVICE_TYPE);
        public void Reannounce() => sd.Announce(serviceProfile);

        private async Task StartGrpcServer()
        {
            KeyCertificatePair kcp = await Task.Run(Authenticator.GetKeyCertificatePair);
            SelectedIP = Utils.GetLocalIPAddress();
            grpcServer = new Grpc.Core.Server() { 
                Services = { Warp.BindService(new GrpcService()) },
                Ports = { new ServerPort(SelectedIP.ToString(), Port, new SslServerCredentials(new List<KeyCertificatePair>() { kcp })) }
            };
            grpcServer.Start();
            log.Info("GRPC started");
        }

        private void StartMDNS(bool flush = false)
        {
            log.Debug("Starting mdns");

            foreach (var a in MulticastService.GetIPAddresses())
            {
                log.Debug($"IP address {a}");
            }
            mdns.NetworkInterfaceDiscovered += (s, e) =>
            {
                foreach (var nic in e.NetworkInterfaces)
                {
                    log.Debug($"discovered NIC '{nic.Name}', id: {nic.Id}");
                }
            };
            sd = new ServiceDiscovery(mdns);
            sd.ServiceInstanceDiscovered += OnServiceInstanceDiscovered;
            sd.ServiceInstanceShutdown += OnServiceInstanceShutdown;
            mdns.AnswerReceived += OnAnswerReceived;

            mdns.Start();
            sd.QueryServiceInstances(SERVICE_TYPE);

            serviceProfile = new ServiceProfile(UUID, SERVICE_TYPE, Port, new List<IPAddress> { SelectedIP });
            serviceProfile.AddProperty("hostname", Utils.GetHostname());
            serviceProfile.AddProperty("type", flush ? "flush" : "real");
            sd.Advertise(serviceProfile);
            sd.Announce(serviceProfile);
        }

        private void PingRemotes()
        {
            foreach (var r in Remotes.Values)
            {
                if (r.Status == RemoteStatus.CONNECTED)
                    r.Ping();
            }
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
                log.Debug($"  Service '{srvName}' has hostname '{server.Target} and port {server.Port}'");
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
            remote.ServiceName = name;
            remote.UUID = name;
            remote.ServiceAvailable = true;

            lock (Remotes)
            {
                if (!Remotes.ContainsKey(name))
                    Remotes.Add(name, remote);
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
