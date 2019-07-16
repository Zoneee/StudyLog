using Jxl.Http;
using Monitor.Common;
using System;
using System.IO;
using System.Net.Http;

namespace Jxl.Logging
{
    /// <summary>
    /// HTTP请求的日志记录器，记录成功的<see cref="HttpRequestMessage"/>与<see cref="HttpResponseMessage"/>信息。
    /// 一个<see cref="HttpRequestMessage"/>请求对应一个文件。
    /// 一个<see cref="HttpClient"/>对应一个日志记录器，但对应多个文件。
    /// 或者说一个<see cref="HttpLoggingHandler"/>对应一个日志记录器，但对应多个文件。
    /// </summary>
    public interface IHttpLogger
    {
        /// <summary>
        /// 记录一次HTTP请求/响应的相关信息
        /// </summary>
        /// <param name="httpMessage">对应的HTTP请求/响应的相关信息</param>
        void Log(HttpMessage httpMessage);
    }

    /// <summary>
    /// 代表一次HTTP请求/响应的相关消息，为不可变的，主要用于记录Http相关日志
    /// </summary>
    [Serializable]
    public sealed class HttpMessage
    {
        /// <summary>
        /// 日志内容
        /// </summary>
        public string LogContent { get; internal set; }

        /// <summary>
        /// 对应的数据类型
        /// </summary>
        public string DataType { get; internal set; }

        internal HttpMessage() { }
    }


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