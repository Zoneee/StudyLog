using Common.HttpException;
using Monitor.Common;
using System;
using System.IO;
using System.Net.Http;

namespace Common.Logging
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
}