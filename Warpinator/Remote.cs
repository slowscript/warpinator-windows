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
        public string ServiceName;
        public string UserName;
        public string Hostname;
        public string DisplayName;
        public string UUID;
        public Bitmap Picture = Properties.Resources.profile;
        public RemoteStatus Status;
        public bool ServiceAvailable;
        public bool IncomingTransferFlag = false;
        public List<Transfer> Transfers = new List<Transfer>();

        ILog log = new Common.Logging.Simple.ConsoleOutLogger("Remote", LogLevel.All, true, false, true, null, true);
        Channel channel;
        Warp.WarpClient client;
        TransferForm form;

        public async void Connect()
        {
            log.Info($"Connecting to {Hostname}");
            Status = RemoteStatus.CONNECTING;
            UpdateUI();
            if (!ReceiveCertificate())
            {
                Status = RemoteStatus.ERROR;
                UpdateUI();
                return;
            }
            log.Trace($"Certificate for {Hostname} received and saved");

            SslCredentials cred = new SslCredentials(Authenticator.GetRemoteCertificate(UUID));
            channel = new Channel(Address.ToString(), Port, cred);
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
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.DeadlineExceeded)
                {
                    log.Debug($"Ping to {Hostname} timed out");
                    Status = RemoteStatus.DISCONNECTED;
                }
                else
                {
                    log.Debug($"Ping to {Hostname} failed");
                    Status = RemoteStatus.ERROR;
                }
                UpdateUI();
            }
        }

        public void StartSendTransfer(Transfer t)
        {
            var opInfo = new OpInfo()
            {
                Ident = Server.current.UUID,
                Timestamp = t.StartTime,
                ReadableName = Server.current.Hostname
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
                ReadableName = Server.current.Hostname
            };
            bool cancelled = false;
            try
            {
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
                    log.Error("Error while receiving", e);
                    //t.errors.add
                    t.Status = TransferStatus.FAILED;
                }
                t.OnTransferUpdated();
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
            Form1.UpdateUI();
        }

        internal void UpdateTransfers()
        {
            if (form != null)
                form.Invoke(new Action(() => form.UpdateTransfers()));
        }

        private async Task<bool> WaitForDuplex()
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

        private bool ReceiveCertificate()
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
                    {
                        udp.Close();
                        break;
                    }
                }
                catch (Exception e)
                {
                    tryCount++;
                    log.Debug("ReceiveCertificate try " + tryCount + " failed", e);
                    Thread.Sleep(1000);
                }
            }
            if (tryCount == 3)
            {
                log.Error($"Failed to receive certificate from {Hostname}");
                return false;
            }
            string base64encoded = Encoding.ASCII.GetString(received);
            byte[] decoded = Convert.FromBase64String(base64encoded);
            if (!Authenticator.SaveRemoteCertificate(decoded, UUID))
            {
                //TODO: Groupcode error
                log.Error("Groupcode error");
                return false;
            }
            return true;
        }
    }
}
