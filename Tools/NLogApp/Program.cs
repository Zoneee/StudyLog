using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Targets;
using NLog.Config;
using NLog.Common;

namespace NLogApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int i = 0;
            LogEventInfo logEventInfo = new LogEventInfo()
            {
                Level = LogLevel.Info,
                Message = (++i).ToString()
            };

            LogEventInfo logEventWarn = new LogEventInfo()
            {
                Level = LogLevel.Warn,
                Message = (++i).ToString()
            };

            LogEventInfo logEventError = new LogEventInfo()
            {
                Level = LogLevel.Error,
                Message = (++i).ToString()
            };

            Loggers.Logger.Log(logEventInfo);
            Console.WriteLine(nameof(logEventInfo));
            Loggers.Logger.Log(logEventInfo);
            Console.WriteLine(nameof(logEventInfo));
            Loggers.Logger.Log(logEventInfo);
            Console.WriteLine(nameof(logEventInfo));
            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            Loggers.Logger.Log(logEventInfo);
            Console.WriteLine(nameof(logEventInfo));
            Console.ReadLine();
        }
    }

    static class Loggers
    {
        public static Logger Logger { get; }
        static Loggers()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }
    }
}

