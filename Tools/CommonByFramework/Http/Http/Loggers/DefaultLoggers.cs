using Monitor.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.Logging
{
    /// <summary>
    /// 默认日志记录者
    /// 日志保存在"程序根目录\GlobalLogs\yyyy-MM-dd\GUID\"目录下
    /// </summary>
    internal class DefaultLogger : ILogger
    {
        public string _path;
        public DefaultLogger(string path = "")
        {
            _path = path;
        }

        public virtual void Log(string message)
        {
            if (string.IsNullOrWhiteSpace(_path))
            {
                _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", DateTime.Now.ToString("yyyy-MM-dd"), Guid.NewGuid().ToString("N"));
            }

            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            File.AppendAllText(Path.Combine(_path, "log.log"), $"{DateTime.Now.ToString("hh:mm:ss")}>>>{message}");
        }
    }

    /// <summary>
    /// 默认HTTP日志记录者
    /// 日志保存在"程序根目录\GlobalLogs\yyyy-MM-dd\GUID\"目录下
    /// </summary>
    internal class DefaultHttpLogger : IHttpLogger
    {
        public string _path;
        public DefaultHttpLogger(string path = "")
        {
            _path = path;
        }

        public void Log(HttpMessage httpMessage)
        {
            if (string.IsNullOrWhiteSpace(_path))
            {
                _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", DateTime.Now.ToString("yyyy-MM-dd"), Guid.NewGuid().ToString("N"));
            }

            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            var path = string.IsNullOrWhiteSpace(httpMessage.DataType) ? Path.Combine(_path, $"{Tool.GetUnixTimestamp()}.html")
                : Path.Combine(_path, $"{Tool.GetUnixTimestamp()}_{httpMessage.DataType}.html");
            File.AppendAllText(path, httpMessage.LogContent);
        }
    }
}
