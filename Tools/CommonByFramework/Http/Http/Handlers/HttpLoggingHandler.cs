using Common.HttpExtension;
using Common.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.HttpHandler
{
    /// <summary>
    /// 负责对某个任务的每次http请求和响应进行日志记录的handler。
    /// 如果在需要发送的<see cref="HttpRequestMessage"/>对象上调用了方法<see cref="HttpRequestMessageExtensions.AddDataType"/>，则会将此次请求和这个数据类型相关联
    /// 保存请求结果页面到单个日志文件
    /// </summary>
    public class HttpLoggingHandler : DelegatingHandler
    {
        /// <summary>
        /// HTTP日志记录器
        /// 用于保存请求结果页面到多个日志文件。只保存成功的，也只能保存成功的
        /// 文件名格式：时间戳_额外名称.html
        /// </summary>
        internal IHttpLogger HttpLogger { get; }

        /// <summary>
        /// 实例化一个HttpLoggingHandler对象，负责指定任务对象的http请求和响应的日志记录
        /// </summary>
        public HttpLoggingHandler(HttpMessageHandler innerHandler, IHttpLogger httpLogger) : base(innerHandler)
        {
            HttpLogger = httpLogger;
        }

        /// <summary>
        /// 发送HTTP请求并记录到日志
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            DateTime startTime = DateTime.Now;

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // HttpConnectionHandler里有一句requestMessage.Headers.Remove(...)，所以如果在另一个线程中遍历Headers可能会报“InvalidOperationException: 集合已修改；可能无法执行枚举操作”。
            // 所以先如下“复制”出来。
            string requestHeadersString = request.Headers.ToString();
            // 由于request.Content会在HttpClient中被dispose，有些方法（比如GetStringAsync）的response.Content也会被HttpClient调用dispose
            // 所以不得不这么做（否则另一个线程访问的时候就可能得到ObjectDisposedException）：
            string requestBody = null;
            if (request.Content != null)
            {
                requestBody = await request.Content.ReadAsStringAsync();
            }
            string responseString = null;
            byte[] responseBytes = null;
            if (response.Content != null)
            {
                // 那些更复杂的content type（比如"video/mp4"）暂且不考虑，只要不是图片就一律以字符串的形式保存，
                // 毕竟大部分情况都是"text/xxx"或"application/json"之类的
                var imageFlag = response.Content.Headers.ContentType?.MediaType.StartsWith("image/") ?? false;
                var streamFlag = response.Content.Headers.ContentType?.MediaType.Contains("octet-stream") ?? false;

                if (imageFlag || streamFlag)
                {
                    responseBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }
                else
                {
                    try
                    {
                        responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    catch (InvalidOperationException) // 有的服务器有问题，会返回一些不正确的编码名，这时候我们就手动给它设为utf-8好了，遇到问题再说
                    {
                        response.Content.Headers.ContentType.CharSet = "utf-8";
                        responseString = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false));
                    }
                }
            }
            var ignored = Log(request, requestHeadersString, requestBody, response, responseBytes, responseString, startTime);
            return response;
        }

        /// <summary>
        /// 手动记录一个HTTP响应正文，<paramref name="description"/>是这个HTTP响应的描述
        /// </summary>
        public void Log(string httpContent, string description)
        {
            HttpLogger?.Log(new HttpMessage() { LogContent = httpContent, DataType = description });
        }

        /// <summary>
        /// 异步记录HTTP响应正文
        /// </summary>
        public Task Log(HttpRequestMessage request, string requestHeadersString, string requestBody, HttpResponseMessage response, byte[] responseBytes, string responseString, DateTime startTime)
        {
            return Task.Run(() =>
            {
                var httpMessage = new HttpMessage();
                StringBuilder sb = GetHeadLogContent(request, requestHeadersString, requestBody, response, startTime);
                if (responseString != null)
                {
                    sb.Append(responseString);
                }
                else if (responseBytes != null)
                {
                    sb.Append(Convert.ToBase64String(responseBytes));
                }

                httpMessage.LogContent = sb.ToString();

                if (request.Properties.TryGetValue(HttpRequestMessageExtensions.Keys.DataType, out var dataType))
                {
                    httpMessage.DataType = dataType.ToString();
                }

                HttpLogger?.Log(httpMessage);
            });
        }

        /// <summary>
        /// 组合Http日志文件中Request与ResponseHead部分
        /// 例：
        /// Start Time:2019/6/17 10:08:54
        /// Consume Time:125.0128毫秒
        /// Url:xxx
        /// Method:GET
        /// Request Headers:
        /// Status Code:200
        /// Response Headers:
        ///    Cache-Control: private
        ///    Server: Microsoft-IIS/7.5
        ///    X-AspNet-Version: 4.0.30319
        ///    X-Powered-By: ASP.NET
        ///    Date: Mon, 17 Jun 2019 02:08:53 GMT
        ///    Content-Type: application/x-javascript; charset=utf-8
        ///    Content-Length: 91
        /// </summary>
        private StringBuilder GetHeadLogContent(HttpRequestMessage request, string requestHeadersString, string requestBody, HttpResponseMessage response, DateTime startTime)
        {
            StringBuilder sb = new StringBuilder(2048);

            void AddHeaders(string headersString)
            {
                var headerStrs = headersString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var headerStr in headerStrs)
                {
                    sb.AppendLine($"    {headerStr}");
                }
            }

            sb.AppendLine("<!--");
            sb.AppendLine($"Start Time:{startTime}");
            sb.AppendLine($"Consume Time:{DateTime.Now.Subtract(startTime).TotalMilliseconds}毫秒");
            sb.AppendLine($"Url:{request.RequestUri.AbsoluteUri}");
            sb.AppendLine($"Method:{request.Method.Method}");
            if (requestBody != null)
            {
                sb.AppendLine($"Param:{requestBody}");
            }
            sb.AppendLine("Request Headers:");
            AddHeaders(requestHeadersString);
            if (request.Content != null)
            {
                AddHeaders(request.Content.Headers.ToString());
            }
            sb.AppendLine($"Status Code:{(int)response.StatusCode}");
            sb.AppendLine("Response Headers:");
            var responseContent = response.Content;
            AddHeaders(response.Headers.ToString());
            if (responseContent != null)
            {
                AddHeaders(responseContent.Headers.ToString());
            }
            sb.AppendLine("-->");

            return sb;
        }
    }
}
