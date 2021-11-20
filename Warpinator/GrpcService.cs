using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warpinator
{
    class GrpcService : Warp.WarpBase
    {
        private static readonly Common.Logging.ILog log = Common.Logging.LogManager.GetLogger<GrpcService>();

        public override Task<HaveDuplex> CheckDuplexConnection(LookupName request, ServerCallContext context)
        {
            bool result = false;
            if (Server.current.Remotes.ContainsKey(request.Id))
            {
                Remote r = Server.current.Remotes[request.Id];
                result = (r.Status == Remote.RemoteStatus.CONNECTED) || (r.Status == Remote.RemoteStatus.AWAITING_DUPLEX);
                if (r.Status == Remote.RemoteStatus.ERROR || r.Status == Remote.RemoteStatus.DISCONNECTED)
                {
                    // TODO: Query new IP address first time this happens
                    // Try reconnecting
                    r.Connect();
                }
            }
            return Task.FromResult (new HaveDuplex() { Response = result });
        }

        public override Task<RemoteMachineInfo> GetRemoteMachineInfo(LookupName request, ServerCallContext context)
        {
            return Task.FromResult(new RemoteMachineInfo { DisplayName = Server.current.DisplayName, UserName = Server.current.UserName });
        }

        public override Task GetRemoteMachineAvatar(LookupName request, IServerStreamWriter<RemoteMachineAvatar> responseStream, ServerCallContext context)
        {
            context.Status = new Status(StatusCode.NotFound, "no picture");
            return null;
        }

        public override Task<VoidType> Ping(LookupName request, ServerCallContext context)
        {
            return Void;
        }

        public override Task<VoidType> ProcessTransferOpRequest(TransferOpRequest request, ServerCallContext context)
        {
            string remoteUUID = request.Info.Ident;
            Remote r = Server.current.Remotes[remoteUUID];
            if (r == null)
            {
                log.Warn($"Received transfer from unknown remote {remoteUUID}");
                return Void;
            }
            log.Info($"Incoming transfer from {r.UserName}");

            var t = new Transfer() {
                Direction = Transfer.TransferDirection.RECEIVE,
                RemoteUUID = remoteUUID,
                StartTime = request.Info.Timestamp,
                Status = Transfer.TransferStatus.WAITING_PERMISSION,
                TotalSize = request.Size,
                FileCount = request.Count,
                SingleMIME = request.MimeIfSingle,
                SingleName = request.NameIfSingle,
                TopDirBaseNames = request.TopDirBasenames.ToList()
            };
            r.Transfers.Add(t);
            t.PrepareReceive();
            
            return Void;
        }

        public override Task<VoidType> CancelTransferOpRequest(OpInfo request, ServerCallContext context)
        {
            log.Debug("Transfer cancelled by the other side");
            var t = GetTransfer(request);
            if (t != null)
                t.MakeDeclined();
            
            return Void;
        }
        public override async Task StartTransfer(OpInfo request, IServerStreamWriter<FileChunk> responseStream, ServerCallContext context)
        {
            log.Debug("Transfer was accepted");
            var t = GetTransfer(request);
            if (t != null)
                await t.StartSending(responseStream);
        }

        public override Task<VoidType> StopTransfer(StopInfo request, ServerCallContext context)
        {
            log.Debug($"Transfer stopped by the other side, error: {request.Error}");
            var t = GetTransfer(request.Info);
            if (t != null)
                t.OnStopped(request.Error);

            return Void;
        }

        private Transfer GetTransfer(OpInfo info)
        {
            if (!Server.current.Remotes.TryGetValue(info.Ident, out Remote r))
            {
                log.Warn("Could not find corresponding remote: " + info.Ident);
                return null;
            }
            var t = r.Transfers.Find((i) => i.StartTime == info.Timestamp);
            if (t == null)
                log.Warn("Could not find corresponding transfer");
            return t;
        }

        private Task<VoidType> Void { get { return Task.FromResult(new VoidType()); } }
    }
}
