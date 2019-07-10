using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jxl.Http
{
    /// <summary>
    /// HttpClient的扩展方法
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// 发起一个Post请求，将响应作为字符串读取并返回，请求的body编码为UTF-8，Content-Type为application/x-www-form-urlencoded
        /// </summary>
        public static Task<string> PostAndReadAsStringAsync(this HttpClient httpClient, string requestUri, string requestBody)
        {
            return PostAndReadAsStringAsync(httpClient, new Uri(requestUri), requestBody);
        }

        /// <summary>
        /// 发起一个Post请求，将响应作为字符串读取并返回，请求的body编码为UTF-8，Content-Type为application/x-www-form-urlencoded
        /// </summary>
        public static Task<string> PostAndReadAsStringAsync(this HttpClient httpClient, Uri requestUri, string requestBody)
        {
            return PostAndReadAsStringAsync(httpClient, requestUri, requestBody, Encoding.UTF8, HttpContentTypes.ApplicationWwwFormUrlEncoded);
        }

        /// <summary>
        /// 发起一个Post请求，将响应作为字符串读取并返回，请求的Content-Type为application/x-www-form-urlencoded
        /// </summary>
        public static Task<string> PostAndReadAsStringAsync(this HttpClient httpClient,
            string requestUri, string requestBody, Encoding requestBodyEncoding)
        {
            return PostAndReadAsStringAsync(httpClient, new Uri(requestUri), requestBody, requestBodyEncoding);
        }

        /// <summary>
        /// 发起一个Post请求，将响应作为字符串读取并返回，请求的Content-Type为application/x-www-form-urlencoded
        /// </summary>
        public static Task<string> PostAndReadAsStringAsync(this HttpClient httpClient,
            Uri requestUri, string requestBody, Encoding requestBodyEncoding)
        {
            return PostAndReadAsStringAsync(httpClient, requestUri, requestBody, requestBodyEncoding, HttpContentTypes.ApplicationWwwFormUrlEncoded);
        }

        /// <summary>
        /// 发起一个Post请求，将响应作为字符串读取并返回
        /// </summary>
        public static Task<string> PostAndReadAsStringAsync(this HttpClient httpClient,
            string requestUri, string requestBody, Encoding requestBodyEncoding, string contentType)
        {
            return PostAndReadAsStringAsync(httpClient, new Uri(requestUri), requestBody, requestBodyEncoding, contentType);
        }

        /// <summary>
        /// 发起一个Post请求，将响应作为字符串读取并返回
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "StringContent不需要释放")]
        public static Task<string> PostAndReadAsStringAsync(this HttpClient httpClient,
            Uri requestUri, string requestBody, Encoding requestBodyEncoding, string contentType)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(requestBody, requestBodyEncoding, contentType)
            };
            return SendAndReadAsStringAsync(httpClient, request);
        }

        /// <summary>
        /// 发起一个HTTP请求，将响应作为字符串读取并返回
        /// </summary>
        public static Task<string> SendAndReadAsStringAsync(this HttpClient httpClient, HttpRequestMessage request)
        {
            return SendAndReadAsStringAsync(httpClient, request, CancellationToken.None);
        }

        /// <summary>
        /// 将指定的HttpRequestMessage参数以Get方法发起HTTP请求，将响应作为字符串读取并返回
        /// </summary>
        public static Task<string> GetStringAsync(this HttpClient httpClient, HttpRequestMessage request)
        {
            if (request.Method != HttpMethod.Get)
            {
                throw new InvalidOperationException("HTTP方法必须是Get！");
            }

            return SendAndReadAsStringAsync(httpClient, request, CancellationToken.None);
        }

        /// <summary>
        /// 本方法只对.Net框架标准的HttpClient实现有效。发起一个HTTP请求，将响应作为字符串读取并返回，并且可以指定超时时间（如果大于HttpClient.Timeout则会提前发生超时）。
        /// 如果超时会抛出<see cref="HttpRequestException"/>。
        /// </summary>
        /// <exception cref="HttpTimeoutException">超过指定的超时时间</exception>
        public static async Task<string> SendAndReadAsStringAsync(this HttpClient httpClient, HttpRequestMessage request, TimeSpan timeout)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(timeout);
                try
                {
                    return await SendAndReadAsStringAsync(httpClient, request, cts.Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    throw new HttpTimeoutException("请求超时。 URL：" + request.RequestUri);
                }
            }
        }

        private static async Task<string> SendAndReadAsStringAsync(this HttpClient httpClient, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (var responseMessage = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
            {
                responseMessage.EnsureSuccessStatusCodeEx();
                if (responseMessage.Content == null)
                {
                    return string.Empty;
                }
                return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }
    }
}
