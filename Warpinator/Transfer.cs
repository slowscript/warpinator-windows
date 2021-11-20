using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging.Simple;

namespace Warpinator
{
    public class Transfer
    {
        ConsoleOutLogger log = new ConsoleOutLogger("Transfer", Common.Logging.LogLevel.All, true, false, true, "", true);
        public enum TransferDirection {
            SEND, RECEIVE
        }
        public enum TransferStatus
        {
            WAITING_PERMISSION, DECLINED, TRANSFERRING, PAUSED, STOPPED,
            FAILED, FINISHED, FINISHED_WITH_ERRORS
        }
        private enum FileType : int {
            FILE = 1, DIRECTORY = 2, SYMLINK = 3
        }
        private const int CHUNK_SIZE = 1024 * 512; //512 kB

        public TransferStatus Status;
        public TransferDirection Direction;
        public string RemoteUUID;
        public ulong StartTime;
        public ulong TotalSize;
        public ulong FileCount;
        public string SingleName = "";
        public string SingleMIME = "";
        public List<string> TopDirBaseNames;

        public bool OverwriteWarning = false;
        public List<string> FilesToSend;
        private List<string> ResolvedFiles;
        private string currentRelativePath;
        private string currentPath;
        private bool cancelled = false;
        internal int id;

        public long BytesTransferred;
        public long BytesPerSecond;
        public long RealStartTime;
        public double Progress { get { return (double)BytesTransferred / TotalSize;  } }
        private long lastTicks;
        private FileStream currentStream;

        /***** SEND & RECEIVE *****/
        public void Stop(bool error = false)
        {
            Server.current.Remotes[RemoteUUID].StopTransfer(this, error);
            OnStopped(error);
        }

        public void OnStopped(bool error)
        {
            if (!error)
                Status = TransferStatus.STOPPED;
            if (Direction == TransferDirection.RECEIVE)
                StopReceiving();
            else StopSending();
            UpdateUI();
        }

        public void MakeDeclined()
        {
            Status = TransferStatus.DECLINED;
            UpdateUI();
        }

        public void UpdateUI()
        {
            Server.current.Remotes[RemoteUUID].UpdateTransfer(id);
        }

        public string GetRemainingTime()
        {
            long now = DateTime.Now.Ticks;
            double avgSpeed = BytesTransferred / ((double)(now - RealStartTime) / TimeSpan.TicksPerSecond);
            int secondsRemaining = (int)((TotalSize - (ulong)BytesTransferred) / avgSpeed);
            return FormatTime(secondsRemaining);
        }

        string FormatTime(int seconds)
        {
            if (seconds > 60)
            {
                int minutes = seconds / 60;
                if (seconds > 3600)
                {
                    int hours = seconds / 3600;
                    minutes -= hours * 60;
                    return $"{hours}h {minutes}m";
                }
                seconds -= minutes * 60;
                return $"{minutes}m {seconds}s";
            }
            else if (seconds > 5)
                return $"{seconds}s";
            else
                return "a few seconds";
        }

        /***** SEND ******/
        public void PrepareSend()
        {
            ResolvedFiles = new List<string>();
            ResolveFiles();
            
            Status = TransferStatus.WAITING_PERMISSION;
            Direction = TransferDirection.SEND;
            StartTime = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds();
            TotalSize = GetTotalSendSize();
            FileCount = (ulong)FilesToSend.Count;
            TopDirBaseNames = new List<string>(FilesToSend.Select((f) => Path.GetFileName(f)));
            if (FileCount == 1)
            {
                SingleName = Path.GetFileName(FilesToSend[0]);
                SingleMIME = System.Web.MimeMapping.GetMimeMapping(SingleName);
            }
        }

        public async Task StartSending(Grpc.Core.IServerStreamWriter<FileChunk> stream)
        {
            Status = TransferStatus.TRANSFERRING;
            RealStartTime = DateTime.Now.Ticks;
            BytesTransferred = 0;
            cancelled = false;
            UpdateUI();

            string f1 = FilesToSend[0];
            int parentLen = f1.TrimEnd(Path.DirectorySeparatorChar).LastIndexOf(Path.DirectorySeparatorChar);
            foreach (var f in ResolvedFiles)
            {
                if (cancelled) break;

                string p = f.Replace(Path.DirectorySeparatorChar, '/'); // For Linux & Android compatibility
                if (Directory.Exists(p))
                {
                    var chunk = new FileChunk()
                    {
                        RelativePath = p.Remove(0,parentLen+1),
                        FileType = (int)FileType.DIRECTORY,
                        FileMode = 493 //0755, C# doesn't have octal literals :(
                        //Time = new FileTime() { Mtime = }
                    };
                    await stream.WriteAsync(chunk);
                }
                else if (File.Exists(p))
                {
                    string relPath = p.Remove(0, parentLen + 1);

                    var fs = File.OpenRead(p);
                    long read = 0; long length = fs.Length;
                    byte[] buf = new byte[CHUNK_SIZE];
                    do //Send at least one chunk for empty files
                    {
                        int r = fs.Read(buf, 0, CHUNK_SIZE);
                        var chunk = new FileChunk()
                        {
                            RelativePath = relPath,
                            FileType = (int)FileType.FILE,
                            FileMode = 420, //0644, C# doesn't have octal literals :(
                            Chunk = Google.Protobuf.ByteString.CopyFrom(buf, 0, r)
                            //Time = new FileTime() { Mtime = }
                        };
                        await stream.WriteAsync(chunk);
                        read += r;
                        BytesTransferred += r;
                        long now = DateTime.Now.Ticks;
                        BytesPerSecond = (long)(r / ((double)(now - lastTicks) / TimeSpan.TicksPerSecond));
                        lastTicks = now;
                        UpdateUI();
                    } while (read < length && !cancelled);
                }
            }
            if (!cancelled)
            {
                Status = TransferStatus.FINISHED;
                UpdateUI();
            }
        }

        private void StopSending()
        {
            cancelled = true;
        }

        private void ResolveFiles()
        {
            foreach (var p in FilesToSend)
            {
                if (Directory.Exists(p))
                    resolveDirectory(p);
                else ResolvedFiles.Add(p);
            }

            void resolveDirectory(string dir)
            {
                ResolvedFiles.Add(dir);
                var dirs = Directory.GetDirectories(dir);
                foreach (var d in dirs)
                    resolveDirectory(d);
                var files = Directory.GetFiles(dir);
                ResolvedFiles.AddRange(files);
            }
        }

        private ulong GetTotalSendSize()
        {
            ulong total = 0;
            foreach (var f in ResolvedFiles)
            {
                if (File.Exists(f))
                    total += (ulong)new FileInfo(f).Length;
            }    
            return total;
        }

        /****** RECEIVE *******/

        public void PrepareReceive()
        {
            //TODO: Check enough space

            //Check if will overwrite
            if (Properties.Settings.Default.AllowOverwrite)
            {
                foreach (string p in TopDirBaseNames)
                {
                    var path = Path.Combine(Properties.Settings.Default.DownloadDir, Utils.SanitizePath(p));
                    if (File.Exists(path) || Directory.Exists(path))
                    {
                        OverwriteWarning = true;
                        break;
                    }
                }
            }

            Server.current.Remotes[RemoteUUID].UpdateTransfers();

            if (Properties.Settings.Default.AutoAccept)
                StartReceiving();
        }

        public void StartReceiving()
        {
            log.Info("Transfer accepted");
            Status = TransferStatus.TRANSFERRING;
            RealStartTime = DateTime.Now.Ticks;
            UpdateUI();
            Server.current.Remotes[RemoteUUID].StartReceiveTransfer(this);
        }

        public void DeclineTransfer()
        {
            log.Info("Transfer declined");
            Server.current.Remotes[RemoteUUID].DeclineTransfer(this);
            MakeDeclined();
        }

        public async Task<bool> ReceiveFileChunk(FileChunk chunk)
        {
            long chunkSize = 0;
            if (chunk.RelativePath != currentRelativePath)
            {
                // End of file
                CloseStream();
                // Begin new file
                currentRelativePath = chunk.RelativePath;
                string sanitizedPath = Utils.SanitizePath(currentRelativePath);
                currentPath = Path.Combine(Server.current.settings.DownloadDir, sanitizedPath);
                if (chunk.FileType == (int)FileType.DIRECTORY)
                    Directory.CreateDirectory(currentPath);
                else if (chunk.FileType == (int)FileType.SYMLINK)
                {
                    log.Warn("Symlinks not supported");
                    //TODO: Display warning
                }
                else
                {
                    if (File.Exists(currentPath))
                        currentPath = HandleFileExists(currentPath);
                    try
                    {
                        currentStream = File.Create(currentPath);
                        var bytes = chunk.Chunk.ToByteArray();
                        await currentStream.WriteAsync(bytes, 0, bytes.Length);
                        chunkSize = bytes.Length;
                    } catch (Exception e)
                    {
                        log.Error($"Failed to open file for writing {currentRelativePath}", e);
                        //Display error
                        FailReceive();
                    }
                }
            }
            else
            {
                try
                {
                    var bytes = chunk.Chunk.ToByteArray();
                    await currentStream.WriteAsync(bytes, 0, bytes.Length);
                    chunkSize = bytes.Length;
                } catch (Exception e)
                {
                    log.Error($"Failed to write to file {currentRelativePath}: {e.Message}");
                    //Display error
                    FailReceive();
                }
            }
            BytesTransferred += chunkSize;
            long now = DateTime.Now.Ticks;
            BytesPerSecond = (long)(chunkSize / ((double)(now - lastTicks) / TimeSpan.TicksPerSecond));
            lastTicks = now;
            UpdateUI();
            return Status == TransferStatus.TRANSFERRING;
        }

        public void FinishReceive()
        {
            log.Debug("Finalizing transfer");
            Status = TransferStatus.FINISHED; //TODO: Finish with errors
            CloseStream();
            UpdateUI();
        }

        private void StopReceiving()
        {
            log.Trace("Stopping receiving");
            CloseStream();
            // Delete incomplete path
            try
            {
                File.Delete(currentPath);
            }
            catch { }
        }

        private void FailReceive()
        {
            //Don't overwrite other reason for stopping
            if (Status == TransferStatus.TRANSFERRING)
            {
                log.Debug("Receiving failed");
                Status = TransferStatus.FAILED;
                Stop(error: true); //Calls stopReceiving & informs other about error
            }
        }

        private string HandleFileExists(string path)
        {
            if (Properties.Settings.Default.AllowOverwrite)
            {
                log.Trace("Overwriting..");
                File.Delete(path);
            }
            else
            {
                var dir = Path.GetDirectoryName(path);
                var file = Path.GetFileNameWithoutExtension(path);
                var ext = Path.GetExtension(path);
                int i = 2;
                do
                {
                    path = Path.Combine(dir, $"{file} ({i}){ext}");
                    i++;
                } while (File.Exists(path));
                log.Trace("New path: " + path);
            }
            return path;
        }

        private void CloseStream()
        {
            if (currentStream != null)
            {
                try
                {
                    currentStream.Dispose();
                }
                catch { }
                currentStream = null;
            }
        }
    }
}
