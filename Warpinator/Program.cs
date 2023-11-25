using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Logging;
using Common.Logging.Simple;

namespace Warpinator
{
    static class Program
    {
        internal static FileLoggerAdapter Log { get; private set; }
        internal static List<string> SendPaths = new List<string>(); 
        static NamedPipeServerStream pipeServer;

        [STAThread]
        static void Main(string[] args)
        {
            //Global logging (libraries)
            var properties = new Common.Logging.Configuration.NameValueCollection
            {
                ["level"] = "INFO",
                ["showLogName"] = "true",
                ["showDateTime"] = "false",
                ["dateTimeFormat"] = "HH:mm:ss.fff"
            };
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(properties);
            //Application logging
            var properties2 = new Common.Logging.Configuration.NameValueCollection
            {
                ["level"] = "ALL",
                ["showLogName"] = "true",
                ["showDateTime"] = "true",
                ["dateTimeFormat"] = "HH:mm:ss.fff"
            };
            Log = new FileLoggerAdapter(args.Contains("-d"), properties2);
            
            var log = Log.GetLogger("Main");
            log.Info("Hola hej!");

            var mutex = new Mutex(true, "warpinator", out bool created);
            // Process arguments
            if (args.Length > 0)
            {
                foreach (var path in args)
                {
                    log.Debug("Got path to send: " + path);
                    if (File.Exists(path) || Directory.Exists(path))
                        SendPaths.Add(path);
                    else log.Warn("Path does not exist");
                }
                if (!created && SendPaths.Count > 0)
                {
                    log.Debug("Passing paths to main process...");
                    using (var pipeClient = new NamedPipeClientStream(".", "warpsendto", PipeDirection.Out))
                    {
                        pipeClient.Connect();
                        using (var sw = new StreamWriter(pipeClient))
                        {
                            sw.AutoFlush = true;
                            foreach (var path in SendPaths)
                                sw.WriteLine(path);
                        }
                    }
                }   
            }
            // Run application if not yet running
            if (created)
            {
                log.Debug("Starting application...");
                Task.Run(RunPipeServer);
                
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());

                pipeServer.Close();
            }
            mutex.Dispose();
            log.Info("Exit");
            Log.Dispose();
        }

        static void RunPipeServer()
        {
            var log = Log.GetLogger("PipeServer");
            pipeServer = new NamedPipeServerStream("warpsendto", PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances);
            try
            {
                using (var sr = new StreamReader(pipeServer))
                {
                    while (true)
                    {
                        pipeServer.WaitForConnection();
                        while (pipeServer.IsConnected)
                        {
                            var path = sr.ReadLine();
                            if (!(File.Exists(path) || Directory.Exists(path)))
                                continue;
                            SendPaths.Add(path);
                            log.Debug($"Got path {path}");
                        }
                        Form1.OnSendTo();
                        pipeServer.Disconnect();
                    }
                }
            }
            catch (Exception e)
            {
                log.Info($"Pipe server quit ({e.GetType()})");
            }
        }
    }
}
