using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Logging;
using Common.Logging.Simple;

namespace Warpinator
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            //Logging
            var properties = new Common.Logging.Configuration.NameValueCollection
            {
                ["level"] = "INFO",
                ["showLogName"] = "true",
                ["showDateTime"] = "false",
                ["dateTimeFormat"] = "HH:mm:ss.fff"

            };
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(properties);
            var log = LogManager.GetLogger("Main");
            log.Info("Hola hej!");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
