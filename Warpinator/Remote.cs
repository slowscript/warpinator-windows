using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Grpc.Core;

namespace Warpinator
{
    public enum RemoteStatus
    {
        CONNECTED,
        DISCONNECTED,
        CONNECTING,
        ERROR,
        AWAITING_DUPLEX
    }
    
    public class Remote
    {
        public IPAddress Address;
        public int Port;
        public int AuthPort = 42001;
        public uint APIVersion = 1;
        public string ServiceName;
        public string UserName;
        public string Hostname;
        public string DisplayName;
        public string UUID;
        public Bitmap Picture = Properties.Resources.profile;
        public RemoteStatus Status;
        public bool ServiceAvailable;
        public bool IncomingTransferFlag = false;
        public bool GroupCodeError = false;
        public List<Transfer> Transfers = new List<Transfer>();
        public event EventHandler RemoteUpdated;

        readonly ILog log = Program.Log.GetLogger("Remote");
        Channel channel;
        Warp.WarpClient client;
        TransferForm form;

        public async void Connect()
        {
            log.Info($"Connecting to {Hostname}, API {APIVersion}");
            Status = RemoteStatus.CONNECTING;
            UpdateUI();
            if (!await ReceiveCertificate())
            {
                Status = RemoteStatus.ERROR;
                UpdateUI();
                return;
            }
            log.Trace($"Certificate for {Hostname} received and saved");

            SslCredentials cred = new SslCredentials(Authenticator.GetRemoteCertificate(UUID));
            var options = APIVersion >= 2 ? new ChannelOption[] {
                new ChannelOption("grpc.keepalive_time_ms", 10000),
                new ChannelOption("grpc.keepalive_timeout_ms", 5000),
                new ChannelOption("grpc.keepalive_permit_without_calls", 1),
                new ChannelOption("grpc.http2.max_pings_without_data", 0),
                new ChannelOption("grpc.http2.min_time_between_pings_ms", 10000),
                new ChannelOption("grpc.http2.min_ping_interval_without_data_ms", 5000)
            } : new ChannelOption[0];
            channel = new Channel(Address.ToString(), Port, cred, options);
            client = new Warp.WarpClient(channel);

            Status = RemoteStatus.AWAITING_DUPLEX;
            UpdateUI();

            if (!await WaitForDuplex())
            {
                log.Error($"Couldn't establish duplex with {Hostname}");
                Status = RemoteStatus.ERROR;
                UpdateUI();
                return;
            }

            Status = RemoteStatus.CONNECTED;

            //Get info
            var info = await client.GetRemoteMachineInfoAsync(new LookupName());
            DisplayName = info.DisplayName;
            UserName = info.UserName;

            // Get avatar
            try
            {
                var avatar = client.GetRemoteMachineAvatar(new LookupName());
                List<byte> bytes = new List<byte>();
                while (await avatar.ResponseStream.MoveNext())
                    bytes.AddRange(avatar.ResponseStream.Current.AvatarChunk);
                Picture = new Bitmap(new MemoryStream(bytes.ToArray()));
            } catch (Exception) {
                Picture = Properties.Resources.profile;
            }

            UpdateUI();
            log.Info($"Connection established with {Hostname}");
        }

        public async void Disconnect()
        {
            log.Info($"Disconnecting {Hostname}");
            await channel.ShutdownAsync();
            Status = RemoteStatus.DISCONNECTED;
        }

        public async void Ping()
        {
            try
            {
                await client.PingAsync(new LookupName() { Id = Server.current.UUID }, deadline: DateTime.UtcNow.AddSeconds(10));
            }
            catch (RpcException)
            {
                log.Debug($"Ping to {Hostname} failed");
                Status = RemoteStatus.DISCONNECTED;
                UpdateUI();
            }
        }

        public void CheckChannelState()
        {
            var s = channel?.State;
            if (s != ChannelState.Ready)
            {
                log.Debug($"Connection to {Hostname} changed state to {s}");
                Status = RemoteStatus.DISCONNECTED;
                UpdateUI();
            }
        }

        public void StartSendTransfer(Transfer t)
        {
            t.UseCompression = Server.current.settings.UseCompression;
            var opInfo = new OpInfo()
            {
                Ident = Server.current.UUID,
                Timestamp = t.StartTime,
                ReadableName = Server.current.Hostname,
                UseCompression = t.UseCompression
            };
            var req = new TransferOpRequest()
            {
                Info = opInfo,
                SenderName = Server.current.DisplayName,
                Receiver = UUID,
                Size = t.TotalSize,
                Count = t.FileCount,
                NameIfSingle = t.SingleName,
                MimeIfSingle = t.SingleMIME
            };
            req.TopDirBasenames.AddRange(t.TopDirBaseNames);
            client.ProcessTransferOpRequestAsync(req);
        }

        public async void StartReceiveTransfer(Transfer t)
        {
            var info = new OpInfo()
            {
                Ident = Server.current.UUID,
                Timestamp = t.StartTime,
                ReadableName = Server.current.Hostname,
                UseCompression = t.UseCompression
            };
            bool cancelled = false;
            try
            {
                t.recvWatch.Restart();
                using (var i = client.StartTransfer(info))
                {
                    while (await i.ResponseStream.MoveNext() && !cancelled)
                    {
                        var chunk = i.ResponseStream.Current;
                        cancelled = !await t.ReceiveFileChunk(chunk);
                    }
                }
                if (!cancelled)
                    t.FinishReceive();
            }
            catch (RpcException e)
            {
                if (e.StatusCode == StatusCode.Cancelled)
                {
                    log.Info("Transfer was reportedly cancelled");
                    t.Status = TransferStatus.STOPPED;
                }
                else
                {
                    log.Error("RPC error while receiving", e);
                    t.errors.Add("Error while receiving. Remote status: " + e.Status.Detail);
                    t.Status = TransferStatus.FAILED;
                }
                t.OnTransferUpdated();
            }
            catch (Exception e)
            {
                log.Error("Fatal error while receiving", e);
                t.errors.Add("Error while receiving: " + e.Message);
                t.Status = TransferStatus.FAILED;
                t.OnTransferUpdated();
            }
            finally
            {
                t.recvWatch.Stop();
            }
        }

        public void DeclineTransfer(Transfer t)
        {
            var info = new OpInfo()
            {
                Ident = Server.current.UUID,
                Timestamp = t.StartTime,
                ReadableName = Server.current.Hostname
            };
            client.CancelTransferOpRequestAsync(info);
        }
        
        public void StopTransfer(Transfer t, bool error)
        {
            var info = new OpInfo()
            {
                Ident = Server.current.UUID,
                Timestamp = t.StartTime,
                ReadableName = Server.current.Hostname
            };
            var stopInfo = new StopInfo()
            {
                Error = error,
                Info = info
            };
            client.StopTransferAsync(stopInfo);
        }

        public void ProcessSendToTransfer()
        {
            if (Program.SendPaths.Count != 0)
            {
                log.Info($"Send To: {Hostname}");
                Transfer t = new Transfer()
                {
                    FilesToSend = Program.SendPaths,
                    RemoteUUID = UUID
                };
                Program.SendPaths = new List<string>();
                Form1.UpdateLabels(); // Revert regular UI
                t.PrepareSend();
                Transfers.Add(t);
                UpdateTransfers();
                StartSendTransfer(t);
            }
        }

        public void OpenWindow()
        {
            if (form == null) {
                form = new TransferForm(this);
                form.Disposed += (a, b) => form = null;
                form.Show();
            }
            form.Focus();
        }

        public void UpdateUI()
        {
            if (form != null)
                form.Invoke(new Action(() => form.UpdateLabels()));
            RemoteUpdated?.Invoke(this, null);
        }

        internal void UpdateTransfers()
        {
            if (form != null)
                form.Invoke(new Action(() => form.UpdateTransfers()));
            RemoteUpdated?.Invoke(this, null); // update RemoteButton in Form1 to indicate incoming transfer
        }

        public void ClearTransfers()
        {
            Transfers.RemoveAll((t) => (t.Status == TransferStatus.FINISHED) || (t.Status == TransferStatus.FINISHED_WITH_ERRORS) ||
              (t.Status == TransferStatus.DECLINED) || (t.Status == TransferStatus.FAILED) || (t.Status == TransferStatus.STOPPED));
        }

        public string GetStatusString()
        {
            switch (Status)
            {
                case RemoteStatus.CONNECTED: return Resources.Strings.connected;
                case RemoteStatus.DISCONNECTED: return Resources.Strings.disconnected;
                case RemoteStatus.CONNECTING: return Resources.Strings.connecting;
                case RemoteStatus.AWAITING_DUPLEX: return Resources.Strings.awaiting_duplex;
                case RemoteStatus.ERROR: return Resources.Strings.error;
                default: return "???";
            }
        }

        private async Task<bool> WaitForDuplex()
        {
            if (APIVersion == 1)
                return await WaitForDuplexV1();
            else
                return await WaitForDuplexV2();
        }
        private async Task<bool> WaitForDuplexV1()
        {
            int tries = 0;
            while (tries < 10)
            {
                try
                {
                    var haveDuplex = await client.CheckDuplexConnectionAsync(new LookupName()
                    {
                        Id = Server.current.UUID,
                        ReadableName = Server.current.Hostname
                    });
                    if (haveDuplex.Response)
                        return true;
                }
                catch (RpcException e)
                {
                    log.Error("Connection interrupted while waiting for duplex", e);
                    return false;
                }
                log.Trace($"Duplex check attempt {tries}: No duplex");
                await Task.Delay(3000);
                tries++;
            }
            return false;
        }
        private async Task<bool> WaitForDuplexV2()
        {
            try
            {
                return (await client.WaitingForDuplexAsync(
                        new LookupName { Id = Server.current.UUID, ReadableName = Server.current.Hostname },
                        deadline: DateTime.UtcNow.AddSeconds(10))
                    ).Response;
            }
            catch (Exception e)
            {
                log.Debug($"Cannot establish duplex: {e.Message}");
                return false;
            }
        }

        private async Task<bool> ReceiveCertificate()
        {
            if (APIVersion == 1)
                return await Task.Run(ReceiveCertificateV1);
            return await ReceiveCertificateV2();
        }
        private bool ReceiveCertificateV1()
        {
            int tryCount = 0;
            byte[] received = null;
            UdpClient udp = new UdpClient();
            udp.Client.ReceiveTimeout = 1000;
            byte[] req = Encoding.ASCII.GetBytes(CertServer.Request);
            IPEndPoint endPoint = new IPEndPoint(Address, Port);
            while (tryCount < 3)
            {
                log.Trace($"Receiving certificate from {Address}, try {tryCount}");
                try
                {
                    udp.Send(req, req.Length, endPoint);
                    IPEndPoint recvEP = new IPEndPoint(0, 0);
                    received = udp.Receive(ref recvEP);
                    if (recvEP.Equals(endPoint))
                        break;
                }
                catch (Exception e)
                {
                    tryCount++;
                    log.Debug("ReceiveCertificate try " + tryCount + " failed: " + e.Message);
                    Thread.Sleep(1000);
                }
            }
            udp.Close();
            if (tryCount == 3)
            {
                log.Error($"Failed to receive certificate from {Hostname}");
                return false;
            }
            string base64encoded = Encoding.ASCII.GetString(received);
            byte[] decoded = Convert.FromBase64String(base64encoded);
            GroupCodeError = !Authenticator.SaveRemoteCertificate(decoded, UUID);
            if (GroupCodeError) {
                log.Error("Groupcode error");
                return false;
            }
            return true;
        }
        private async Task<bool> ReceiveCertificateV2()
        {
            log.Trace($"Receiving certificate from {Hostname} (APIv2)");
            var authChannel = new Channel(Address.ToString(), AuthPort, ChannelCredentials.Insecure);
            var regClient = new WarpRegistration.WarpRegistrationClient(authChannel);
            var certResp = await regClient.RequestCertificateAsync(
                new RegRequest { Hostname = Server.current.Hostname, Ip = Server.current.SelectedIP.ToString() },
                deadline: DateTime.UtcNow.AddSeconds(20)
            );
            _ = authChannel.ShutdownAsync();
            byte[] decoded = Convert.FromBase64String(certResp.LockedCert);
            GroupCodeError = !Authenticator.SaveRemoteCertificate(decoded, UUID);
            if (GroupCodeError)
            {
                log.Error("Groupcode error");
                return false;
            }
            return true;
        }
    }
}
