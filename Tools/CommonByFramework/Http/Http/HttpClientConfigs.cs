using Common;
using Common.HttpException;
using Common.HttpHandler;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common.HttpExtension
{
    /// <summary>
    /// 提供<see cref="HttpClient"/>配置信息简单管理
    /// </summary>
    public class HttpClientConfigs
    {


        /// <summary>
        /// 创建<see cref="HttpClient"/>对象
        /// 推荐在不需要控制HTTP细节时使用
        /// </summary>
        public static HttpClient CreateHttpClient()
        {
            return CreateHttpClient(string.Empty);
        }

        /// <summary>
        /// 创建<see cref="HttpClient"/>对象
        /// 推荐在不需要控制HTTP细节时使用
        /// </summary>
        /// <param name="path">日志保存目录</param>
        public static HttpClient CreateHttpClient(string path)
        {
            var logger = new DefaultLogger(path);
            var connectionHandler = new HttpConnectionHandler(logger);
            var httpLogger = new DefaultHttpLogger(path);
            var loggingHandler = new HttpLoggingHandler(connectionHandler, httpLogger);
            var retryHandler = new HttpRetryHandler(loggingHandler, logger);
            return new HttpClient(retryHandler);
        }

        /// <summary>
        /// 创建<see cref="HttpClient"/>对象
        /// 推荐在需要控制HTTP细节时使用，不修改Handler顺序时
        /// 实际上现在 LoggerHandler与RetryHandler 的顺序已经不重要了
        /// LoggerHandler只记录成功的页面
        /// RetryHandler只记录“失败”信息
        /// </summary>
        /// <param name="senior">HTTP细节对象</param>
        public static HttpClient CreateHttpClient(HttpClientConfigs senior)
        {
            return new HttpClient(senior.RetryHandler);
        }

        /// <summary>
        /// 默认配置信息
        /// 使用<see cref="DefaultLogger"/>和<see cref="DefaultHttpLogger"/>
        /// 处理顺序：<see cref="ConnectionHandler"/>=><see cref="RetryHandler"/>=><see cref="LoggingHandler"/>
        /// </summary>
        public HttpClientConfigs()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GlobalLogs", DateTime.Now.ToString("yyyy-MM-dd"), Guid.NewGuid().ToString("N"));
            Logger = new DefaultLogger(path);
            ConnectionHandler = new HttpConnectionHandler(Logger);
            HttpLogger = new DefaultHttpLogger(path);
            LoggingHandler = new HttpLoggingHandler(ConnectionHandler, HttpLogger);
            RetryHandler = new HttpRetryHandler(LoggingHandler, Logger);
        }

        /// <summary>
        /// 默认配置信息
        /// 使用<see cref="DefaultLogger"/>和<see cref="DefaultHttpLogger"/>
        /// 处理顺序：<see cref="ConnectionHandler"/>=><see cref="RetryHandler"/>=><see cref="LoggingHandler"/>
        /// </summary>
        /// <param name="path">日志目录</param>
        public HttpClientConfigs(string path)
        {
            Logger = new DefaultLogger(path);
            ConnectionHandler = new HttpConnectionHandler(Logger);
            HttpLogger = new DefaultHttpLogger(path);
            LoggingHandler = new HttpLoggingHandler(ConnectionHandler, HttpLogger);
            RetryHandler = new HttpRetryHandler(LoggingHandler, Logger);
        }

        /// <summary>
        /// 处理顺序：<see cref="ConnectionHandler"/>=><see cref="RetryHandler"/>=><see cref="LoggingHandler"/>
        /// </summary>
        /// <param name="logger">通常日志记录者。记录一些类似“开始任务”或“进入特殊处理”之类的日志</param>
        /// <param name="httpLogger">HTTP日志记录者。每个Request与Response的头和内容都被记录在单独的文件中</param>
        public HttpClientConfigs(ILogger logger, IHttpLogger httpLogger)
        {
            Logger = logger;
            HttpLogger = httpLogger;

            ConnectionHandler = new HttpConnectionHandler(Logger);
            LoggingHandler = new HttpLoggingHandler(ConnectionHandler, HttpLogger);
            RetryHandler = new HttpRetryHandler(LoggingHandler, Logger);
        }

        public ILogger Logger { get; set; }
        public IHttpLogger HttpLogger { get; set; }
        public HttpConnectionHandler ConnectionHandler { get; set; }
        public HttpRetryHandler RetryHandler { get; set; }
        public HttpLoggingHandler LoggingHandler { get; set; }
    }
}
