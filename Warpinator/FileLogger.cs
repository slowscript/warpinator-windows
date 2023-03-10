using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Simple;

namespace Warpinator
{
    internal class FileLogger : AbstractSimpleLogger
    {
        readonly StreamWriter sw;
        internal FileLogger(StreamWriter _sw, string logName, LogLevel logLevel, bool showLevel, bool showDateTime, bool showLogName, string dateTimeFormat)
            : base(logName, logLevel, showLevel, showDateTime, showLogName, dateTimeFormat)
        {
            sw = _sw;
        }

        protected override void WriteInternal(LogLevel level, object message, Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            FormatOutput(sb, level, message, exception);
            string s = sb.ToString();
            Console.WriteLine(s);
            if (sw != null)
                sw.WriteLine(s);
        }
    }

    internal class FileLoggerAdapter : AbstractSimpleLoggerFactoryAdapter, IDisposable
    {
        StreamWriter sw = null;
        internal FileLoggerAdapter(bool logToFile, Common.Logging.Configuration.NameValueCollection conf) : base(conf)
        {
            if (!logToFile)
                return;
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Warpinator"); //Cant use utils yet
            string log1 = Path.Combine(path, "latest.log");
            string log2 = Path.Combine(path, "previous.log");
            if (File.Exists(log1) && (new FileInfo(log1).Length > 1024*1024))
            {
                File.Delete(log2);
                File.Move(log1, log2);
            }
            sw = new StreamWriter(log1, true);
        }

        public void Dispose()
        {
            sw?.Dispose();
        }

        protected override ILog CreateLogger(string name, LogLevel level, bool showLevel, bool showDateTime, bool showLogName, string dateTimeFormat)
        {
            return new FileLogger(sw, name, level, showLevel, showDateTime, showLogName, dateTimeFormat);
        }
    }
}
