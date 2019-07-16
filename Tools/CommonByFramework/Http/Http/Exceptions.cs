using System;
using System.Net.Http;

namespace Jxl.Http
{
    /// <summary>
    /// 表示等待HTTP响应超时
    /// </summary>
    [Serializable]
    public class HttpTimeoutException : HttpRequestException
    {
        /// <summary>
        /// 实例化一个HTTP超时异常
        /// </summary>
        public HttpTimeoutException() { }

        /// <summary>
        /// 实例化一个HTTP超时异常
        /// </summary>
        public HttpTimeoutException(string message) : base(message) { }

        /// <summary>
        /// 实例化一个HTTP超时异常，并且指定一个内部异常表示造成错误的源头
        /// </summary>
        public HttpTimeoutException(string message, Exception inner) : base(message, inner) { }
    }
}
