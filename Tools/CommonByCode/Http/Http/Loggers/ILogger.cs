using Common.HttpException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common.Logging
{
    /// <summary>
    /// 日志记录器，记录需要记录的日志信息。如果需要记录HTTP请求信息到文件请使用<see cref="IHttpLogger"/>。
    /// 一个<see cref="HttpClient"/>对应一个日志记录器。
    /// 或者说一个<see cref="HttpLoggingHandler"/>对应一个日志记录器。
    /// 因为<see cref="HttpClient"/>总会携带一个唯一的<see cref="HttpLoggingHandler"/>所以上面两句话表示相同的意思。
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录日志信息
        /// </summary>
        /// <param name="message">日志信息</param>
        void Log(string message);
    }
}
