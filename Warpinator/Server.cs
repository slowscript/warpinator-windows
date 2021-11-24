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
        ILog log = new Common.Logging.Simple.ConsoleOutLogger("Server", LogLevel.All, true, false, true, "", true);
        const string SERVICE_TYPE = "_warpinator._tcp";

        public static Server current;

        public string DisplayName;
        public string UserName;
        public string Hostname;
        public ushort Port = 42000;
        public string UUID;
        public string ProfilePicture;
        public bool NotifyIncoming;
        public bool Running = false;
        public string SelectedInterface;

        public Dictionary<string, Remote> Remotes = new Dictionary<string, Remote>();

        Grpc.Core.Server grpcServer;
        readonly ServiceDiscovery sd;
        readonly MulticastService mdns;
        ServiceProfile serviceProfile;
        readonly ConcurrentDictionary<string, ServiceRecord> mdnsServices = new ConcurrentDictionary<string, ServiceRecord>();
        readonly ConcurrentDictionary<string, IPAddress> hostnameDict = new ConcurrentDictionary<string, IPAddress>();
        internal Properties.Settings settings = Properties.Settings.Default;
        Timer pingTimer = new Timer(10_000);

        public Server()
        {
            current = this;
            DisplayName = System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName;
            Hostname = Environment.MachineName;
            UserName = Environment.UserName;
            
            //Load settings
            settings = Properties.Settings.Default;
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

            mdns = new MulticastService((ifaces) => ifaces.Where((iface) => SelectedInterface == null || iface.Id == SelectedInterface));
            mdns.UseIpv6 = false;
            sd = new ServiceDiscovery(mdns);
            pingTimer.Elapsed += (a, b) => PingRemotes();
            pingTimer.AutoReset = true;
        }

        public void Start()
        {
            log.Info("-- Starting server");
            Running = true;
            if (String.IsNullOrEmpty(settings.NetworkInterface))
                SelectedInterface = null;
            else SelectedInterface = settings.NetworkInterface;
            StartGrpcServer(); //Also initializes authenticator for certserver
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
            Start();
        }

        public void Rescan() => sd.QueryServiceInstances(SERVICE_TYPE);
        public void Reannounce() => sd.Announce(serviceProfile);

        private void StartGrpcServer()
        {
            KeyCertificatePair kcp = Authenticator.GetKeyCertificatePair();
            grpcServer = new Grpc.Core.Server() { 
                Services = { Warp.BindService(new GrpcService()) },
                Ports = { new ServerPort(Utils.GetLocalIPAddress().ToString(), Port, new SslServerCredentials(new List<KeyCertificatePair>() { kcp })) }
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
            sd.ServiceInstanceDiscovered += OnServiceInstanceDiscovered;
            sd.ServiceInstanceShutdown += OnServiceInstanceShutdown;
            mdns.AnswerReceived += OnAnswerReceived;

            mdns.Start();
            sd.QueryServiceInstances(SERVICE_TYPE);

            serviceProfile = new ServiceProfile(UUID, SERVICE_TYPE, Port, new List<IPAddress> { Utils.GetLocalIPAddress() });
            serviceProfile.AddProperty("hostname", Utils.GetHostname());
            serviceProfile.AddProperty("type", flush ? "flush" : "real");
            sd.Advertise(serviceProfile);
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
            log.Debug($"Service discovered: '{e.ServiceInstanceName}'");
            if (!mdnsServices.ContainsKey(e.ServiceInstanceName.ToString()))
                mdnsServices.TryAdd(e.ServiceInstanceName.ToString(), new ServiceRecord() { FullName = e.ServiceInstanceName.ToString() });
        }
        
        private void OnServiceInstanceShutdown(object sender, ServiceInstanceShutdownEventArgs e)
        {
            log.Debug($"Service lost: '{e.ServiceInstanceName}'");
            if (Remotes.ContainsKey(e.ServiceInstanceName.ToString()))
            {
                var r = Remotes[e.ServiceInstanceName.ToString()];
                r.ServiceAvailable = false;
                r.UpdateUI();
            }
        }

        private void OnAnswerReceived(object sender, MessageEventArgs e)
        {
            log.Debug("-- Answer:");
            
            var servers = e.Message.Answers.OfType<SRVRecord>();
            foreach (var server in servers)
            {
                log.Debug($"  Service '{server.Name}' has hostname '{server.Target} and port {server.Port}'");
                if (!mdnsServices.ContainsKey(server.CanonicalName))
                    mdnsServices.TryAdd(server.CanonicalName, new ServiceRecord { FullName = server.Name.ToString() });
                mdnsServices[server.CanonicalName].Hostname = server.Target.ToString();
                if (hostnameDict.TryGetValue(server.Target.ToString(), out IPAddress addr))
                    mdnsServices[server.CanonicalName].Address = addr;
                mdnsServices[server.CanonicalName].Port = server.Port;
                mdns.SendQuery(server.Target, type: DnsType.A);
            }

            var addresses = e.Message.Answers.OfType<AddressRecord>();
            foreach (var address in addresses)
            {
                if (address.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    log.Debug($"  Hostname '{address.Name}' resolves to {address.Address}");
                    hostnameDict.AddOrUpdate(address.Name.ToString(), address.Address, (a, b) => address.Address);
                    var svc = mdnsServices.Values.Where((s) => (s.Hostname == address.Name.ToString())).FirstOrDefault();
                    if (svc != null)
                        svc.Address = address.Address;
                }
            }

            var txts = e.Message.Answers.OfType<TXTRecord>();
            foreach (var txt in txts)
            {
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

            Remotes.Add(name, remote);
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
