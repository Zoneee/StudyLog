using Common.Extension;
using Common.HttpException;
using Common.HttpExtension;
using Common.HttpHead;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Common.HttpHandler
{
    /// <summary>
    /// 负责HTTP重试，内部一般为<see cref="HttpConnectionHandler"/>。
    /// 可以通过<see cref="ResponseChecker"/>自定义全局重试检测规则，设置为null则不执行，
    /// 如果通过<see cref="HttpRequestMessageExtensions.AddResponseChecker"/>设置，该<see cref="HttpRequestMessage"/>将不会运行<see cref="ResponseChecker"/>。
    /// 可以通过<see cref="RequestExceptionHandler"/>自定义请求异常回调函数，，设置为null则不执行，
    /// 如果通过<see cref="HttpRequestMessageExtensions.AddRequestExceptionHandler"/>设置，该<see cref="HttpRequestMessage"/>将不会运行<see cref="RequestExceptionHandler"/>。
    /// 可以通过<see cref="LimitTimes"/>自定义请求重试次数，
    /// 如果通过<see cref="HttpRequestMessageExtensions.AddLimitTimes"/>设置，该<see cref="HttpRequestMessage"/>的<see cref="LimitTimes"/>将会被覆盖。
    /// </summary>
    public class HttpRetryHandler : DelegatingHandler
    {
        public HttpRetryHandler(HttpMessageHandler innerHandler, ILogger logger) : base(innerHandler)
        {
            Logger = logger;
        }

        public HttpRetryHandler(HttpMessageHandler innerHandler, ILogger logger, Func<HttpRequestException, Task> defaultRequestExceptionHandler) : base(innerHandler)
        {
            Logger = logger;
            s_defaultRequestExceptionHandler = defaultRequestExceptionHandler;
        }

        public HttpRetryHandler(HttpMessageHandler innerHandler, ILogger logger, Func<HttpResponseMessage, Task> defaultResponseCheckerHandler) : base(innerHandler)
        {
            Logger = logger;
            s_defaultResponseCheckerHandler = defaultResponseCheckerHandler;
        }

        public HttpRetryHandler(HttpMessageHandler innerHandler, ILogger logger, Func<HttpRequestException, Task> defaultRequestExceptionHandler, Func<HttpResponseMessage, Task> defaultResponseCheckerHandler) : base(innerHandler)
        {
            Logger = logger;
            s_defaultRequestExceptionHandler = defaultRequestExceptionHandler;
            s_defaultResponseCheckerHandler = defaultResponseCheckerHandler;
        }

        /// <summary>
        /// 日志记录器
        /// 用于保存日志信息到单个日志文件。会保存一般性信息，也会保存警告性信息，包括“超时、取消请求、内容不合适”等信息
        /// 记录到“log.log”文件中
        /// </summary>
        internal ILogger Logger { get; }

        /// <summary>
        /// 3xx自动跳转
        /// </summary>
        public bool AllowAutoRedirect { get; set; } = true;

        /// <summary>
        /// 重试间隔时间
        /// </summary>
        public int DelayTime { get; set; } = 0;

        protected int _limitTimes = 2;
        /// <summary>
        /// 重试次数
        /// 不小于0且不大于20
        /// </summary>
        public int LimitTimes
        {
            get { return _limitTimes; }
            set
            {
                if (value < 0 || value > 20)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _limitTimes = value;
            }
        }

        /// <summary>
        /// 超时时间
        /// 到达该限时不代表真正的HTTP超时时间，代表将结果舍弃了
        /// 毫秒
        /// </summary>
        public int Timeout { get; set; } = 1000 * 20;

        /// <summary>
        /// 请求异常句柄
        /// 在请求发生异常后，调用的处理函数
        /// </summary>
        public Func<HttpRequestException, Task> RequestExceptionHandler { get; set; } = s_defaultRequestExceptionHandler;
        /// <summary>
        /// 默认的请求异常句柄
        /// 默认就是什么都不做
        /// </summary>
        private static Func<HttpRequestException, Task> s_defaultRequestExceptionHandler = _ => Task.FromResult(true);

        /// <summary>
        /// 重试检测
        /// 自定义Response检查规则，用于自定义重试，处理类似：“网站繁忙，请稍后再试”情况
        /// </summary>
        public Func<HttpResponseMessage, Task> ResponseChecker { get; set; } = s_defaultResponseCheckerHandler;
        /// <summary>
        /// 默认重试检测
        /// 检测HTTP状态码是否是3xx跳转
        /// </summary>
        private static Func<HttpResponseMessage, Task> s_defaultResponseCheckerHandler = _ =>
        {
            var status = (int)_.StatusCode;
            if (!Regex.IsMatch(status.ToString(), "^3[0-9]{2}$"))
            {
                _.EnsureSuccessStatusCodeEx();
            }
            return Task.FromResult(true);
        };

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var limitTimes = GetUserDefinedLimitTimes(request);
            var timeout = GetUserDefinedTimeout(request);

            for (int i = 0; ; i++)
            {
                if (DelayTime > 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(DelayTime)).ConfigureAwait(false);
                }

                try
                {
                    using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout)))
                    {
                        //当到达自定义“超时”或HttpClient超时时，都会被标记为超时
                        using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
                        {
                            var response = await base.SendAsync(request, linkedCts.Token).ConfigureAwait(false);

                            //处理3xx跳转
                            for (int j = 0; ; j++)
                            {
                                if (j > 10)
                                {
                                    //抛出HttpRequestException是为了与其他异常区分，记录日志
                                    throw new HttpRequestException("自动跳转次数过多！");
                                }

                                //创建新的Request
                                var allowAutoRedirect = GetUserDefinedAllowAutoRedirect(request);
                                if (allowAutoRedirect && IsRedirection(response.StatusCode) && response.Headers.Location != null)
                                {
                                    var location = response.Headers.Location;
                                    if (!location.IsAbsoluteUri)
                                    {
                                        location = new Uri(response.RequestMessage.RequestUri, location);
                                    }
                                    var redirectRequest = new HttpRequestMessage(HttpMethod.Get, location);
                                    foreach (var header in request.Headers)
                                    {
                                        // 其实只需要把HttpClient.DefaultRequestHeaders中的Headers加进来就行，但是没办法分辨，
                                        // 所以就这样了，万一遇到由于加了多余的Header而无法请求成功的情况，就手动跳转吧。
                                        if (header.Key != HttpKnownHeaderNames.Cookie)
                                        {
                                            redirectRequest.Headers.Add(header.Key, header.Value);
                                        }
                                    }
                                    foreach (var prop in request.Properties)
                                    {
                                        redirectRequest.Properties.Add(prop.Key, prop.Value);
                                    }
                                    response = await base.SendAsync(redirectRequest, linkedCts.Token).ConfigureAwait(false);
                                }
                                else
                                {
                                    break;
                                }

                            }

                            //处理检测
                            var responseChecker = ResponseChecker;
                            if (request.Properties.TryGetValue(HttpRequestMessageExtensions.Keys.ResponseChecker, out var obj))
                            {
                                responseChecker = (Func<HttpResponseMessage, Task>)obj;
                            }

                            //没有检测机制就直接返回
                            if (responseChecker == null)
                            {
                                return response;
                            }

                            //检测后再返回，如果没通过则会抛出HttpRequestException
                            await responseChecker(response).ConfigureAwait(false);
                            return response;
                        }
                    }
                }
                catch (HttpTimeoutException ex)
                {
                    //因为ResponseChecker不一定总是执行，所以再添加一次
                    ex.SetUrl(request.RequestUri.AbsoluteUri);

                    if (cancellationToken.IsCancellationRequested) throw;// 如果是HttpClient那边传过来的cancellationToken，说明总的Timeout时间到了，所以直接throw
                    if (i == limitTimes) throw;
                    await ExceRequestExceptionHandler(i, ex, request).ConfigureAwait(false);
                }
                catch (HttpRequestException ex)
                {
                    //因为ResponseChecker不一定总是执行，所以再添加一次
                    ex.SetUrl(request.RequestUri.AbsoluteUri);

                    if (i == limitTimes) throw;
                    await ExceRequestExceptionHandler(i, ex, request).ConfigureAwait(false);
                }
            }
        }

        protected bool IsRedirection(HttpStatusCode statusCode)
        {
            var status = (int)statusCode;
            return Regex.IsMatch(status.ToString(), "^3[0-9]{2}$");
        }

        /// <summary>
        /// 获取用户自定义RequestExceptionHandler
        /// 如果有自定义RequestExceptionHandler，将不会执行<see cref="RequestExceptionHandler"/>
        /// </summary>
        protected async Task ExceRequestExceptionHandler(int limitTimes, HttpRequestException e, HttpRequestMessage request)
        {
            //没有自定义将执行默认
            var exHandler = RequestExceptionHandler;
            //获取自定义规则
            if (request.Properties.TryGetValue(HttpRequestMessageExtensions.Keys.RequestExceptionHandler, out var userDefindHandler))
            {
                exHandler = (Func<HttpRequestException, Task>)userDefindHandler;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append($"Url：{request.RequestUri}   ");
            builder.Append($"Method：{request.Method.ToString()}   ");
            builder.Append(request.Method == HttpMethod.Post ? $"Param：{await request.Content.ReadAsStringAsync()}   " : string.Empty);

            //没有默认将直接抛出
            if (exHandler == null)
            {
                Logger.Log($"HTTP请求失败{++limitTimes}次。原因：{e.GetFullMessage()} {builder.ToString()}。备注：取消执行RequestExceptionHandler");
                throw e;
            }

            Logger.Log($"HTTP请求失败{++limitTimes}次。原因：{e.GetFullMessage()} {builder.ToString()}");
            await exHandler(e).ConfigureAwait(false);
        }

        /// <summary>
        /// 获取用户自定义LimitTimes
        /// </summary>
        protected int GetUserDefinedLimitTimes(HttpRequestMessage request)
        {
            var i = LimitTimes;
            if (request.Properties.TryGetValue(HttpRequestMessageExtensions.Keys.LimitTimes, out var limitTimes))
            {
                i = (int)limitTimes;
            }
            return i;
        }

        /// <summary>
        /// 获取用户自定义Timeout
        /// </summary>
        protected int GetUserDefinedTimeout(HttpRequestMessage request)
        {
            var i = Timeout;
            if (request.Properties.TryGetValue(HttpRequestMessageExtensions.Keys.Timeout, out var timeout))
            {
                i = (int)timeout;
            }
            return i;
        }

        /// <summary>
        /// 获取用户自定义AllowAutoRedirect
        /// </summary>
        protected bool GetUserDefinedAllowAutoRedirect(HttpRequestMessage request)
        {
            var i = AllowAutoRedirect;
            if (request.Properties.TryGetValue(HttpRequestMessageExtensions.Keys.Timeout, out var redirect))
            {
                i = (bool)redirect;
            }
            return i;
        }
    }
}
