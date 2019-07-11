using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebSocketApp
{
    class ConsoleLogWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
        public TextBox TextBox { get; private set; }
        public LoggerHelper Logger { get; private set; }
        public ConsoleLogWriter(TextBox textBox)
        {
            TextBox = textBox;
            Logger = LoggerHelper.CreateLoggerHelper();
        }

        public override void WriteLine(string value)
        {
            var m = $"{DateTime.Now.ToString("hh:mm:ss")}  {value}{Environment.NewLine}";
            if (TextBox.InvokeRequired)
            {
                TextBox.Invoke(new Action(() =>
                {
                    TextBox.AppendText(m);
                }));
            }
            else
            {
                TextBox.AppendText(m);
            }
            //base.WriteLine(value);
            Logger.Log(LogLevel.Info, value);
        }
    }

    class LoggerHelper
    {
        public Logger Logger;
        public string Path;
        private LoggerHelper() { }

        public static LoggerHelper CreateLoggerHelper()
        {
            var logger = new LoggerHelper();
            logger.Logger = LogManager.GetCurrentClassLogger();
            logger.Path = System.IO.Path.Combine(DateTime.Now.ToString("yyyy-MM-dd"), "log.log");
            return logger;
        }

        public static LoggerHelper CreateLoggerHelper(string path)
        {
            var logger = new LoggerHelper();
            logger.Logger = LogManager.GetCurrentClassLogger();
            logger.Path = path;
            return logger;
        }

        public void Log(LogLevel level, string message)
        {
            LogEventInfo logEventInfo = new LogEventInfo()
            {
                Level = level,
                Message = message,
            };
            logEventInfo.Properties["path"] = Path;
            Logger.Log(logEventInfo);
        }
    }
}
