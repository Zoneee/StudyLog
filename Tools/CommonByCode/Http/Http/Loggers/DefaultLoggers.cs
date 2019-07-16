using Monitor.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Common.Logging
{
    internal class NLogger
    {
        static NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Log(LogLevel level, string message, string path)
        {
            LogEventInfo logEventInfo = new LogEventInfo()
            {
                Level = level,
                Message = message,
            };
            logEventInfo.Properties["path"] = path;
            Logger.Log(logEventInfo);
        }
    }

    /// <summary>
    /// 默认日志记录者
    /// 日志保存在"程序根目录\GlobalLogs\yyyy-MM-dd\GUID\"目录下
    /// </summary>
    internal class DefaultLogger : ILogger
    {
        public string _path;
        /// <summary>
        /// 默认日志记录者
        /// </summary>
        /// <param name="path">日志保存路径。指定到文件</param>
        public DefaultLogger(string path = "")
        {
            if (string.IsNullOrWhiteSpace(path))
                _path = Path.Combine(Guid.NewGuid().ToString("N"), "log.log");
            else
                _path = path;
        }

        public void Log(LogLevel logLevel, string message)
        {
            message = $"{DateTime.Now.ToString("hh:mm:dd")}{logLevel.Name}|{message}";
            Log(message);
        }

        public virtual void Log(string message)
        {
            NLogger.Log(LogLevel.Info, message, _path);
        }
    }

    /// <summary>
    /// 默认HTTP日志记录者
    /// 日志保存在"程序根目录\GlobalLogs\yyyy-MM-dd\GUID\"目录下
    /// </summary>
    internal class DefaultHttpLogger : IHttpLogger
    {
        public string _path;
        /// <summary>
        /// 默认日志记录者
        /// </summary>
        /// <param name="path">日志保存目录。指定到目录</param>
        public DefaultHttpLogger(string path = "")
        {
            if (string.IsNullOrWhiteSpace(path))
                _path = Path.Combine(Guid.NewGuid().ToString("N"));
            else
                _path = path;
        }

        public virtual void Log(HttpMessage httpMessage)
        {
            var path = string.IsNullOrWhiteSpace(httpMessage.DataType) ? Path.Combine(_path, $"{Tool.GetUnixTimestamp()}.html")
                : Path.Combine(_path, $"{Tool.GetUnixTimestamp()}_{httpMessage.DataType}.html");

            NLogger.Log(LogLevel.Info, httpMessage.LogContent, path);
        }
    }
}
