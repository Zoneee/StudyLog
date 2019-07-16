using System.Net.Http;
using System.Threading.Tasks;

namespace Common.HttpExtension
{
    /// <summary>
    ///HTTP相关的扩展方法
    /// </summary>
    public static class HttpExtensions
    {
        internal static class Keys
        {
            /// <summary>
            /// 请求url
            /// </summary>
            internal static readonly string Url = "url";
            /// <summary>
            /// 可读信息
            /// </summary>
            internal static readonly string SourceForLog = "sourceForLog";
        }

        #region Response

        /// <summary>
        /// 类似<see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>，但是抛出的异常包含更多信息
        /// </summary>
        public static HttpResponseMessage EnsureSuccessStatusCodeEx(this HttpResponseMessage response)
        {
            try
            {
                return response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                ex.Data[Keys.Url] = response.RequestMessage.RequestUri;//这个是自定义添加的，因为记录日志时需要
                throw;
            }
        }

        /// <summary>
        /// 内部实现：先response.EnsureSuccessStatusCodeEx()，然后保证Content不为null的前提下，返回response.Content.ReadAsStringAsync()，否则返回空字符串
        /// </summary>
        public static Task<string> ReadContentAsStringAsync(this HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCodeEx();
            if (response.Content != null)
            {
                return response.Content.ReadAsStringAsync();
            }
            return Task.FromResult(string.Empty);
        }

        /// <summary>
        /// 内部实现：先response.EnsureSuccessStatusCodeEx()，然后保证Content不为null的前提下，返回response.Content.ReadAsByteArrayAsync()，否则返回空byte数组
        /// </summary>
        public static Task<byte[]> ReadContentAsByteArrayAsync(this HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCodeEx();
            if (response.Content != null)
            {
                return response.Content.ReadAsByteArrayAsync();
            }
            return Task.FromResult(new byte[0]);
        }

        #endregion

        #region Request

        /// <summary>
        /// 获取一个HttpRequestException对应的Url
        /// </summary>
        public static string GetUrl(this HttpRequestException requestException)
        {
            return requestException.Data[Keys.Url]?.ToString() ?? string.Empty;
        }

        /// <summary>
        ///为一个HttpRequestException设置对应的Url，返回此HttpRequestException
        /// </summary>
        public static HttpRequestException SetUrl(this HttpRequestException requestException, string url)
        {
            requestException.Data[Keys.Url] = url;
            return requestException;
        }

        #region HttpRequestException

        /// <summary>
        /// 为HTTP请求异常设置一个“来源（可读信息）”，用于记录日志
        /// </summary>
        public static void SetSourceForLog(this HttpRequestException requestException, string sourceForLog)
        {
            if (requestException.Data.Contains(Keys.SourceForLog)) return;
            requestException.Data[Keys.SourceForLog] = sourceForLog;
        }

        /// <summary>
        /// 获取HTTP异常的可读信息
        /// </summary>
        internal static string GetSourceForLog(this HttpRequestException requestException)
        {
            // 若不存在key，则返回null
            return (string)requestException.Data[Keys.SourceForLog];
        }

        #endregion HttpRequestException


        #endregion

    }
}
