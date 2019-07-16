using Common.HttpException;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common.HttpExtension
{
    /// <summary>
    /// HttpRequestMessage的扩展方法
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        internal static class Keys
        {
            internal static readonly string DataType = "DataType";
            internal static readonly string LimitTimes = "LimitTimes";
            internal static readonly string Timeout = "Timeout";
            internal static readonly string AllowAutoRedirect = "AllowAutoRedirect";
            internal static readonly string ResponseChecker = "ResponseChecker";
            internal static readonly string RequestExceptionHandler = "RequestExceptionHandler";
        }

        /// <summary>
        /// 为请求添加总尝试次数限制
        /// </summary>
        /// <param name="httpRequest">HTTP请求</param>
        /// <param name="limitTimes">总尝试次数限制</param>
        public static void AddLimitTimes(this HttpRequestMessage httpRequest, int limitTimes)
        {
            if (limitTimes < 1 || limitTimes > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(limitTimes));
            }
            httpRequest.Properties.Add(Keys.LimitTimes, limitTimes);
        }

        /// <summary>
        /// 为请求添加超时限制，单位毫秒
        /// </summary>
        public static void AddTimeout(this HttpRequestMessage httpRequest, int millisecondsDelay)
        {
            httpRequest.Properties.Add(Keys.Timeout, millisecondsDelay);
        }

        /// <summary>
        /// 为请求添加额外名称
        /// </summary>
        /// <param name="httpRequest">HTTP请求</param>
        /// <param name="exName">额外名称</param>
        public static void AddDataType(this HttpRequestMessage httpRequest, string exName)
        {
            if (string.IsNullOrWhiteSpace(exName))
            {
                throw new ArgumentNullException(nameof(exName));
            }
            httpRequest.Properties.Add(Keys.DataType, exName);
        }

        /// <summary>
        /// 设置是否允许自动跳转
        /// </summary>
        /// <param name="httpRequest">HTTP请求</param>
        /// <param name="allowAutoRedirect">是否允许自动跳转</param>
        public static void SetAllowAutoRedirect(this HttpRequestMessage httpRequest, bool allowAutoRedirect)
        {
            httpRequest.Properties.Add(Keys.AllowAutoRedirect, allowAutoRedirect);
        }

        /// <summary>
        /// 为HTTP请求添加响应检测函数。如果认为检测不通过（即需要重试），则必须抛出<see cref="HttpRequestException"/>。
        /// 如果responseChecker为null则不会进行重试。注意，如果处理异常需要一定耗时（比如换代理或者休眠），则最好把处理逻辑放在ExceptionHandler中。
        /// </summary>
        /// <param name="httpRequest">HTTP请求</param>
        /// <param name="responseChecker">响应检测函数。</param>
        public static void AddResponseChecker(this HttpRequestMessage httpRequest, Func<HttpResponseMessage, Task> responseChecker)
        {
            httpRequest.Properties.Add(Keys.ResponseChecker, responseChecker);
        }

        /// <summary>
        /// 为HTTP请求添加请求异常或超时（即<see cref="HttpRequestException"/>或<see cref="HttpTimeoutException"/>发生时）的处理函数。
        /// 如果requestExceptionHandler为null则不会进行重试。
        /// </summary>
        /// <param name="httpRequest">HTTP请求</param>
        /// <param name="requestExceptionHandler">请求异常处理函数</param>
        public static void AddRequestExceptionHandler(this HttpRequestMessage httpRequest, Func<HttpRequestException, Task> requestExceptionHandler)
        {
            httpRequest.Properties.Add(Keys.RequestExceptionHandler, requestExceptionHandler);
        }
    }
}
