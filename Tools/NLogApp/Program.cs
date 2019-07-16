using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Targets;
using NLog.Config;
using NLog.Common;
using Common.HttpExtension;
using System.IO;

namespace NLogApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = HttpClientConfigs.CreateHttpClient();

            for (int httpNum = 0; httpNum < 21; httpNum++)
            {
                await client.GetAsync("https://baidu.com");
            }


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

            Console.WriteLine("Next FlushTimeout Test");
            Console.ReadLine();
            //test flushTimeout
            Globals.Logger.Log(logEventInfo);
            Console.WriteLine(nameof(logEventInfo));
            Console.WriteLine("Delay 5s");
            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            Globals.Logger.Log(logEventInfo);
            Console.WriteLine();

            Console.WriteLine("Next BufferSize Test");
            Console.ReadLine();
            //test bufferSize
            Globals.Logger.Log(logEventInfo);
            Console.WriteLine(nameof(logEventInfo));
            Globals.Logger.Log(logEventInfo);
            Console.WriteLine(nameof(logEventInfo));
            Globals.Logger.Log(logEventInfo);
            Console.WriteLine(nameof(logEventInfo));
            Console.WriteLine(nameof(logEventInfo));
            Console.WriteLine();

            Console.WriteLine("Next Default ColorConsole Test");
            Console.ReadLine();
            //test default ColorConsole
            Tasks.Logger.Log(logEventInfo);
            Tasks.Logger.Log(logEventWarn);
            Tasks.Logger.Log(logEventError);
            Console.WriteLine();

            Console.WriteLine("Next User-defined ColorConsole Test");
            Console.ReadLine();
            //test condition="level >= LogLevel.Error and contains(message,'serious')"
            Tasks.Logger.Log(LogLevel.Error, "serious test");
            //test level >=LogLevel.Error
            Tasks.Logger.Log(LogLevel.Error, "test");
            //test starts-with(logger,'Example')
            Examples.ColorConsole.Logger.Log(LogLevel.Info, "Example");


            Console.ReadLine();
        }
    }

    /// <summary>
    /// global - BufferingWrapper
    /// </summary>
    static class Globals
    {
        public static Logger Logger { get; }
        static Globals()
        {
            Logger = LogManager.GetCurrentClassLogger();
            Logger.SetProperty("path", Path.Combine(DateTime.Now.ToString("yyyy-MM-dd"), "log.log"));
        }
    }

    /// <summary>
    /// console - ColoredConsole
    /// </summary>
    static class Tasks
    {
        public static Logger Logger { get; }
        static Tasks()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }
    }


}

/// <summary>
/// console - ColoredConsole - starts-with(logger,'Example')
/// </summary>
namespace Examples
{
    static class ColorConsole
    {
        public static Logger Logger { get; }
        static ColorConsole()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }
    }
}

