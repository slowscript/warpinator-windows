using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;

namespace Warpinator
{
    public enum TransferStatus
    {
        WAITING_PERMISSION, DECLINED, TRANSFERRING, PAUSED, STOPPED,
        FAILED, FINISHED, FINISHED_WITH_ERRORS
    }
    public enum TransferDirection {
        SEND, RECEIVE
    }
    
    public class Transfer
    {
        readonly ILog log = Program.Log.GetLogger("Transfer");
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
        public bool UseCompression = false;
        public string Message;

        public bool OverwriteWarning = false;
        public List<string> FilesToSend;
        private List<string> ResolvedFiles;
        private string currentRelativePath;
        private string currentPath;
        private DateTime? currentFileDateTime;
        private bool cancelled = false;
        public event EventHandler TransferUpdated;
        public List<string> errors = new List<string>();

        public long BytesTransferred;
        public TransferSpeed BytesPerSecond = new TransferSpeed();
        public long RealStartTime;
        public double Progress { get { return TotalSize == 0 ? 0.0 : (double)BytesTransferred / TotalSize; } }
        private double lastMillis;
        internal Stopwatch recvWatch = new Stopwatch();
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
            OnTransferUpdated();
        }

        public void MakeDeclined()
        {
            Status = TransferStatus.DECLINED;
            OnTransferUpdated();
        }

        public void OnTransferUpdated()
        {
            TransferUpdated?.Invoke(this, null);
        }

        public string GetRemainingTime()
        {
            long now = DateTime.UtcNow.Ticks;
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
                return Resources.Strings.a_few_seconds;
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
            FileCount = (ulong)ResolvedFiles.Count;
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
            RealStartTime = DateTime.UtcNow.Ticks;
            BytesTransferred = 0;
            cancelled = false;
            OnTransferUpdated();
            stream.WriteOptions = new Grpc.Core.WriteOptions((Grpc.Core.WriteFlags)0x4); //Write through, but doesnt seem to be doing much
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            double lastMillis = 0;

            log.Trace($"Compression enabled: {UseCompression}");
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
                        RelativePath = p.Remove(0, parentLen + 1),
                        FileType = (int)FileType.DIRECTORY,
                        FileMode = 493 //0755, C# doesn't have octal literals :(
                    };
                    await stream.WriteAsync(chunk);
                }
                else if (File.Exists(p))
                {
                    string relPath = p.Remove(0, parentLen + 1);

                    var fs = File.OpenRead(p);
                    long read = 0;
                    long length = fs.Length;
                    byte[] buf = new byte[CHUNK_SIZE];
                    bool firstChunk = true;
                    do //Send at least one chunk for empty files
                    {
                        int r = fs.Read(buf, 0, CHUNK_SIZE);
                        byte[] chunkData = buf;
                        int chunkLen = r;
                        if (UseCompression) {
                            chunkData = ZLibCompressor.Compress(buf);
                            chunkLen = chunkData.Length;
                        }
                        FileTime ftime = null;
                        if (firstChunk)
                        {
                            firstChunk = false;
                            DateTime mtime = File.GetLastWriteTime(p);
                            ftime = new FileTime() {
                                Mtime = (ulong)new DateTimeOffset(mtime).ToUnixTimeSeconds(),
                                MtimeUsec = (uint)mtime.Millisecond * 1000
                            };
                        }

                        var chunk = new FileChunk()
                        {
                            RelativePath = relPath,
                            FileType = (int)FileType.FILE,
                            FileMode = 420, //0644, C# doesn't have octal literals :(
                            Chunk = Google.Protobuf.ByteString.CopyFrom(chunkData, 0, chunkLen),
                            Time = ftime
                        };
                        await stream.WriteAsync(chunk);
                        read += r;
                        BytesTransferred += r;
                        double now = stopwatch.Elapsed.TotalMilliseconds;
                        double bps = (1000 * r) / (now - lastMillis);
                        BytesPerSecond.Add((long)bps);
                        //Console.WriteLine($"{bps/1024}, avg {BytesPerSecond.GetMovingAverage()/1024} (read {r} in {now - lastMillis} ms)");
                        lastMillis = now;
                        OnTransferUpdated();
                    } while (read < length && !cancelled);
                    fs.Close();
                }
            }
            stopwatch.Stop();
            if (!cancelled)
            {
                Status = TransferStatus.FINISHED;
                OnTransferUpdated();
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

            if (Properties.Settings.Default.AutoAccept)
                StartReceiving();
        }

        public void StartReceiving()
        {
            log.Info("Transfer accepted");
            log.Trace($"Compression enabled: {UseCompression}");
            Status = TransferStatus.TRANSFERRING;
            RealStartTime = DateTime.UtcNow.Ticks;
            lastMillis = 0;
            BytesPerSecond.Receiving = true;
            OnTransferUpdated();
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
                if (currentFileDateTime.HasValue)
                    File.SetLastWriteTime(currentPath, currentFileDateTime.Value);
                currentFileDateTime = null;
                // Begin new file
                currentRelativePath = chunk.RelativePath;
                string sanitizedPath = Utils.SanitizePath(currentRelativePath);
                currentPath = Path.Combine(Properties.Settings.Default.DownloadDir, sanitizedPath);
                if (!ValidatePath(currentPath))
                    throw new ArgumentException("Path leads outside download dir");
                if (chunk.FileType == (int)FileType.DIRECTORY)
                    Directory.CreateDirectory(currentPath);
                else if (chunk.FileType == (int)FileType.SYMLINK)
                {
                    log.Warn("Symlinks not supported");
                    errors.Add(Resources.Strings.symlinks_not_supported);
                }
                else
                {
                    if (File.Exists(currentPath))
                        currentPath = HandleFileExists(currentPath);
                    if (chunk.Time != null)
                        currentFileDateTime = DateTimeOffset.FromUnixTimeSeconds((long)chunk.Time.Mtime).LocalDateTime;
                    try
                    {
                        currentStream = File.Create(currentPath);
                        var bytes = chunk.Chunk.ToByteArray();
                        if (UseCompression)
                            bytes = ZLibCompressor.Decompress(bytes);
                        await currentStream.WriteAsync(bytes, 0, bytes.Length);
                        chunkSize = bytes.Length;
                    } catch (Exception e)
                    {
                        log.Error($"Failed to open file for writing {currentRelativePath}", e);
                        errors.Add(Resources.Strings.failed_open_file + currentRelativePath);
                        FailReceive();
                    }
                }
            }
            else
            {
                try
                {
                    var bytes = chunk.Chunk.ToByteArray();
                    if (UseCompression)
                        bytes = ZLibCompressor.Decompress(bytes);
                    await currentStream.WriteAsync(bytes, 0, bytes.Length);
                    chunkSize = bytes.Length;
                } catch (Exception e)
                {
                    log.Error($"Failed to write to file {currentRelativePath}: {e.Message}");
                    errors.Add(String.Format(Resources.Strings.failed_write_file, currentFileDateTime, e.Message));
                    FailReceive();
                }
            }
            BytesTransferred += chunkSize;
            double now = recvWatch.Elapsed.TotalMilliseconds;
            double bps = (1000 * chunkSize) / (now - lastMillis);
            BytesPerSecond.Add((long)bps);
            //Console.WriteLine($"{bps / 1024}, avg {BytesPerSecond.GetMovingAverage() / 1024} (read {chunkSize} in {now - lastMillis} ms)");
            lastMillis = now;
            OnTransferUpdated();
            return Status == TransferStatus.TRANSFERRING;
        }

        public void FinishReceive()
        {
            log.Debug("Finalizing transfer");
            if (errors.Count > 0)
                Status = TransferStatus.FINISHED_WITH_ERRORS;
            else Status = TransferStatus.FINISHED;
            CloseStream();
            if (currentFileDateTime.HasValue)
                File.SetLastWriteTime(currentPath, currentFileDateTime.Value);
            OnTransferUpdated();
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
            catch (Exception e)
            {
                log.Warn("Could not delete incomplete file: " + e.Message);
            }
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

        private bool ValidatePath(string path)
        {
            return Utils.NormalizePath(path).StartsWith(Utils.NormalizePath(Properties.Settings.Default.DownloadDir));
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

        internal string GetStatusString()
        {
            switch (Status) {
                case TransferStatus.WAITING_PERMISSION: return Resources.Strings.waiting_for_permission;
                case TransferStatus.DECLINED: return Resources.Strings.declined;
                case TransferStatus.TRANSFERRING: return Resources.Strings.transferring;
                case TransferStatus.PAUSED: return Resources.Strings.paused;
                case TransferStatus.STOPPED: return Resources.Strings.stopped;
                case TransferStatus.FINISHED: return Resources.Strings.finished;
                case TransferStatus.FINISHED_WITH_ERRORS: return Resources.Strings.finished_errors;
                case TransferStatus.FAILED: return Resources.Strings.failed;
                default: return "???";
            }
        }
    }

    public class TransferSpeed
    {
        const int HistoryLength = 24;

        public bool Receiving = false;
        int idx = 0;
        int count = 0;
        long[] history = new long[HistoryLength];

        public void Add(long bps)
        {
            history[idx] = bps;
            idx = (idx + 1) % HistoryLength;
            if (count < HistoryLength)
                count++;
        }

        public long GetMovingAverage()
        {
            if (count == 0)
                return 0;
            else if (count == 1)
                return history[0];
            if (Receiving)
                return history.Sum() / count;
            
            //Calculate trimmed mean (avoid outliers when write returned immediately)
            long[] sorted = new long[count];
            Array.Copy(history, sorted, count);
            Array.Sort(sorted);
            int trimLength = (int)(count * 0.8);
            Span<long> trimmed = new Span<long>(sorted, 0, trimLength);
            return trimmed.ToArray().Sum() / trimLength;
        }
    }
}
