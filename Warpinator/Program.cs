using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Logging;
using Common.Logging.Simple;

namespace Warpinator
{
    static class Program
    {
        internal static ILoggerFactoryAdapter Log { get; private set; }
        internal static string SendPath;
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
                ["showDateTime"] = "false",
            };
            Log = new ConsoleOutLoggerFactoryAdapter(properties2);
            
            var log = Log.GetLogger("Main");
            log.Info("Hola hej!");

            if (args.Length > 0)
            {
                var path = args[0];
                log.Trace("Got path to send: " + path);
                if (File.Exists(args[0]) || Directory.Exists(args[0]))
                    SendPath = path;
                else log.Warn("Path does not exist");
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
