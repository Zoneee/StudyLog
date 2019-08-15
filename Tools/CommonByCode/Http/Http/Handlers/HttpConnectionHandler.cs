using Common.Extension;
using Common.HttpException;
using Common.HttpExtension;
using Common.HttpHead;
using Common.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Common.HttpHandler
{
    /// <summary>
    /// 对System.Net.Http.HttpMessageHandler的简单轻量级实现，由System.Net.Http.HttpClient使用。
    /// 已处理<see cref="CookieException"/>，因为需要将Cookie添加在<see cref="CookieContainer"/>中，但服务端可能对Cookie的实现并不规范。
    /// 已处理<see cref="WebException"/>，因为该类型异常可以重试，将其包装为<see cref="HttpTimeoutException"/>或<see cref="HttpRequestException"/>传递到<see cref="HttpRetryHandler"/>
    /// </summary>
    public sealed class HttpConnectionHandler : HttpMessageHandler
    {
        private static readonly RequestCachePolicy s_defaultCachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

        static HttpConnectionHandler()
        {
            ServicePointManager.DnsRefreshTimeout = 20000;
            ServicePointManager.MaxServicePointIdleTime = 20000;
            ServicePointManager.DefaultConnectionLimit = 512; // 默认是2，其实设置为50肯定足够了。
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;//始终通过
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12; //| SecurityProtocolType.Ssl3;
        }

        private long _maxResponseSize = 10 * 1024 * 1024;

        /// <summary>
        /// 对System.Net.Http.HttpMessageHandler的简单轻量级实现，由System.Net.Http.HttpClient使用。
        /// </summary>
        /// <param name="logger">异常日志记录器</param>
        public HttpConnectionHandler(ILogger logger) : base()
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Properties

        /// <summary>
        /// 日志记录器
        /// </summary>
        //public Action<string> Logger { get; set; } = _ => { };
        public ILogger Logger { get; }

        /// <summary>
        /// 对响应进行自动解压缩的解压缩方法，默认为DecompressionMethods.None。
        /// </summary>
        public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.None;

        /// <summary>
        /// 缓存策略，默认为RequestCacheLevel: NoCacheNoStore。
        /// </summary>
        public RequestCachePolicy CachePolicy { get; set; } = s_defaultCachePolicy;

        /// <summary>
        /// 安全证书，默认为Count为0的X509CertificateCollection。
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "HttpWebRequest也是这样的")]
        public System.Security.Cryptography.X509Certificates.X509CertificateCollection ClientCertificates { get; set; }

        /// <summary>
        /// 用来存放cookies的System.Net.CookieContainer。
        /// </summary>
        public CookieContainer CookieContainer { get; set; } = new CookieContainer();

        /// <summary>
        /// 使用的代理，默认为null。
        /// </summary>
        public WebProxy Proxy { get; set; }

        /// <summary>
        /// 是否开启http持久连接（Connection: keep-alive），默认为true。
        /// </summary>
        public bool KeepAlive { get; set; } = true;

        /// <summary>
        /// 默认的User-Agent请求头，对所有通过此实例的请求都有效，并且优先级小于<see cref="HttpClient.DefaultRequestHeaders"/>以及<see cref="HttpRequestMessage.Headers"/>。
        /// HttpClient相关的类会对User-Agent中的不合法字符做检测，这里不会。而且通过这里设置的User-Agent头无法在HTTP日志里显示。
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 响应body的最大字节长度限制，默认10 M
        /// </summary>
        public long MaxResponseSize
        {
            get => _maxResponseSize;
            set
            {
                if (value < 1 || value > 40 * 1014 * 1024)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxResponseSize));
                }
                _maxResponseSize = value;
            }
        }

        #endregion Properties

        /// <summary>
        /// 发送HTTP请求，返回HTTP响应
        /// </summary>
        // 方法的实现主要在于把HttpRequestMessage转换成HttpWebRequest，以及把HttpWebResponse转换成HttpResponseMessage。
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            HttpWebRequest request = CreateRequest(requestMessage);

            try
            {
                using (cancellationToken.Register(() => request.Abort(), useSynchronizationContext: false))
                {
                    //添加请求的body，注意：对HttpWebRequest.GetRequestStream返回的stream的操作可能会通过网络，所以这一步应该算在超时时间范围内
                    if (requestMessage.Method == HttpMethod.Post && requestMessage.Content != null)
                    {
                        using (Stream requestBody = await request.GetRequestStreamAsync().ConfigureAwait(false))
                        {
                            await requestMessage.Content.CopyToAsync(requestBody).ConfigureAwait(false);
                        }
                    }

                    HttpWebResponse response = null;
                    try
                    {
                        try
                        {
                            response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
                        }
                        catch (WebException ex)
                        {
                            //HttpClient在默认情况下（也就是用HttpClientHandler时），如果服务端返回了一个非200的HTTP状态码，
                            //那么返回HttpResponseMessage的方法是不会抛出HttpRequestException的，所以这里保持一致。
                            if (ex.Status != WebExceptionStatus.ProtocolError)
                            {
                                throw;
                            }
                            response = (HttpWebResponse)ex.Response;
                        }

                        HttpResponseMessage responseMessage = new HttpResponseMessage(response.StatusCode) { RequestMessage = requestMessage };
                        //添加响应的body
                        using (Stream stream = response.GetResponseStream())
                        {
                            responseMessage.Content = new StreamContent(stream);
                            // 下面这句LoadIntoBufferAsync是为了让上层的“ReadAs...”方法都是从内存中读取，而不是直接从底层的socket。
                            // 然后就可以把response stream给dispose掉了，并且上层在调用ReadAs...时也不会有“Stream Disposed Exception”。
                            // HttpClient在默认情况下也会调用LoadIntoBufferAsync。
                            await responseMessage.Content.LoadIntoBufferAsync(_maxResponseSize).ConfigureAwait(false);
                        }

                        //添加响应的headers
                        for (int i = 0; i < response.Headers.Count; ++i)
                        {
                            string key = response.Headers.Keys[i];
                            string value = response.Headers[i];
                            if (!responseMessage.Headers.TryAddWithoutValidation(key, value))
                            {
                                responseMessage.Content.Headers.TryAddWithoutValidation(key, value);
                            }
                        }

                        string setCookieValue = response.Headers.Get(HttpKnownHeaderNames.SetCookie);
                        if (setCookieValue != null)
                        {
                            // 必须把Expires中的逗号都去掉，不然会错乱。而且用response.Headers.GetValues也没用，有些机器可以，有些机器就是不行，可能是.Net Framework版本问题。
                            setCookieValue = ConvertExpires(setCookieValue);
                            var setCookies = setCookieValue.Split(',');
                            string lastCookie = null;
                            foreach (var setCookie in setCookies)
                            {
                                var attributes = setCookie.Split(';');
                                var separator = new[] { '=' };
                                var cookie = attributes[0].Split(separator, 2);

                                try
                                {
                                    // 很不幸的是，.Net对cookie的解析有问题：万一cookie的key或value里带逗号，那么会被“分割”成多个cookie，因为它的实现是用逗号当做分隔符的
                                    // 而且SetCookie或者Add方法都没用。考虑到RFC中规定cookie值中确实不能带逗号，简单起见，实现如下。
                                    // 这样只适用于“只有value中有且只有一个逗号”的情况，其他情况（比如逗号在key中、或是原始的Set-Cookie中本来就没有等号）都不适用
                                    if (cookie.Length == 1 && !string.IsNullOrEmpty(lastCookie))// 比如“key=a,b”会被分割成“key=a”和“b”，所以后者的length等于1
                                    {
                                        // 这时候把当前cookie替换成上一个cookie，Domain什么的就能对得起来了，否则上一个cookie的Domain什么的都是空
                                        CookieContainer?.SetCookies(request.RequestUri, lastCookie + ";" + string.Join(";", attributes.Skip(1)));
                                        continue;
                                    }
                                    lastCookie = attributes[0];

                                    string cookieHeader = setCookie;

                                    // 如果set-cookie中没有指定domain，那么我们就需要手动加上“前缀的点”
                                    // 比如foo.com返回的cookie，在www.foo.com也应该是能使用的，如果不加前缀点，则再次请求www.foo.com时则不会带这个cookie
                                    // Skip(1)是因为Set-Cookie的第一项总是“<cookie-name>=<cookie-value>”
                                    // CookieContainer.Add和CookieContainer.SetCookies都是只有在新的cookie的name、path和domain都相同的情况下，才会覆盖老的cookie
                                    var temp = attributes.Skip(1).Select(c => c.Split(separator, 2)).ToArray();
                                    if (!temp.Any(attribute => attribute[0].Trim().ToLower() == "domain"))
                                    {
                                        if (cookie.Length > 1)
                                        {
                                            cookieHeader = setCookie.TrimEnd(';') + $";domain=.{requestMessage.RequestUri.Host}";
                                        }
                                    }
                                    // 在Set-Cookie中没有显示指定path的情况下，CookieContainer会把path设为当前请求的Uri的path
                                    // 但是Chrome中会把path设为"/"，所以为了保持一致，这里还得手动修改path为"/"：
                                    if (!temp.Any(attribute => attribute[0].Trim().ToLower() == "path"))
                                    {
                                        cookieHeader = setCookie.TrimEnd(';') + ";path=/";
                                    }

                                    CookieContainer?.SetCookies(requestMessage.RequestUri, cookieHeader);
                                }
                                catch (CookieException ex) // 这个一般都是服务端的错误，比如cookie中有非法符号等。
                                {
                                    Logger.Log($"{ex.GetFullMessage()}({setCookie})");
                                }
                            }
                        }

                        return responseMessage;
                    }
                    finally
                    {
                        response?.Close(); // 参考：https://stackoverflow.com/questions/9627669/getrequeststream-is-throwing-time-out-exception-when-posting-data-to-https-url/10117605#10117605
                        response?.Dispose(); // 保险起见还是调用一下吧
                    }
                }
            }
            catch (WebException ex)
            {
                // 将WebException转为HttpRequestException，从而保持与HttpClient和HttpMessageHandler一致。
                // 但HttpClient默认实现中，超时或取消会抛出TaskCanceledException，但是这里没有保持一致是因为，如果在这里抛出
                // TaskCanceledException，那么它会在HttpClient内部被捕获并替换（.Net 4.5下），导致这里自定义的Message和Data都丢失了
                HttpRequestException httpRequestException;
                if (ex.Status == WebExceptionStatus.RequestCanceled || ex.Status == WebExceptionStatus.Timeout)
                {
                    httpRequestException = new HttpTimeoutException("请求超时。", ex);
                }
                else
                {
                    httpRequestException = new HttpRequestException(ex.Message, ex);
                }
                httpRequestException.Data[HttpExtensions.Keys.Url] = requestMessage.RequestUri;
                throw httpRequestException;
            }
        }

        private HttpWebRequest CreateRequest(HttpRequestMessage requestMessage)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestMessage.RequestUri);

            // 这里必须设成false，否则如果返回302，并且有一些“特殊”的cookie，那么就进不到我们的对这些特殊cookie的特殊处理代码了。
            request.AllowAutoRedirect = false;
            request.Pipelined = true;
            request.AutomaticDecompression = AutomaticDecompression;
            if (CachePolicy != null)
            {
                request.CachePolicy = CachePolicy;
            }
            if (ClientCertificates != null)
            {
                request.ClientCertificates = ClientCertificates;
            }
            request.Proxy = Proxy;
            request.KeepAlive = KeepAlive;
            request.Method = requestMessage.Method.Method;
            request.ProtocolVersion = requestMessage.Version;

            // 如果直接用request.CookieContainer = CookieContainer，那么HttpLoggingHandler中无法访问到Cookie头，所以如下手动添加
            string cookieHeader = CookieContainer?.GetCookieHeader(requestMessage.RequestUri);
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                // 注意，HttpRequestMessage的设计初衷是不能被重复使用的，但是在HttpRetryHandler却可以被重复使用，所以一定要先Remove（没有提供“Update”方法）
                requestMessage.Headers.Remove(HttpKnownHeaderNames.Cookie);
                requestMessage.Headers.Add(HttpKnownHeaderNames.Cookie, cookieHeader);
            }

            // 某些header只能通过属性设置，如果通过Add方法添加则会抛出ArgumentException
            // 参考：https://docs.microsoft.com/en-us/dotnet/api/system.net.httpwebrequest.headers?view=netframework-4.5
            foreach (var header in requestMessage.Headers)
            {
                switch (header.Key)
                {
                    case HttpKnownHeaderNames.Accept:
                        request.Accept = string.Join(", ", header.Value);
                        continue;
                    case HttpKnownHeaderNames.UserAgent:
                        request.UserAgent = string.Join(" ", header.Value);
                        continue;
                    case HttpKnownHeaderNames.Host:
                        request.Host = header.Value.FirstOrDefault();
                        continue;
                    case HttpKnownHeaderNames.Referer:
                        request.Referer = header.Value.FirstOrDefault();
                        continue;
                    case HttpKnownHeaderNames.Cookie:
                        request.Headers.Add(header.Key, string.Join("; ", header.Value));
                        continue;
                    default:
                        break;
                }

                request.Headers.Add(header.Key, string.Join(", ", header.Value)); // 其他未知的头先用逗号分隔吧，遇到问题再说
            }

            if (request.UserAgent == null) // 注意上面也设置过一次request.UserAgent，如果设置过则不为null
            {
                // 这里也不要尝试为requestMessage.Headers更新UserAgent，否则可能抛出异常
                request.UserAgent = UserAgent;
            }

            if (requestMessage.Content != null)
            {
                foreach (var header in requestMessage.Content.Headers)
                {
                    switch (header.Key)
                    {
                        case HttpKnownHeaderNames.ContentLength:
                            request.ContentLength = long.Parse(header.Value.Single());
                            continue;
                        case HttpKnownHeaderNames.ContentType:
                            request.ContentType = header.Value.Single();
                            continue;
                        default:
                            break;
                    }
                    request.Headers.Add(header.Key, string.Join("; ", header.Value)); // 其他未知的头先用分号分隔吧，遇到问题再说
                }
            }

            // 不要发送Expect: 100-continue这个头，有时候代理服务器不接受这个头
            request.ServicePoint.Expect100Continue = false;

            return request;
        }

        private static readonly Regex s_regex = new Regex("(e|E)xpires=(.*?)(;|$|=)"); // 等号是指下一个Set-Cookie中的等号

        internal string ConvertExpires(string setCookie)
        {
            return s_regex.Replace(setCookie, match =>
            {
                // 各种可能的情况参考测试项目中的测试样例，总之我们要做的就是消除逗号
                var expiresValue = match.Groups[2].Value;
                if (match.Groups[3].Value == "=")
                {
                    expiresValue = match.Groups[2].Value.Substring(0, expiresValue.LastIndexOf(","));
                }

                if (expiresValue == string.Empty) // Expires的值为空表示浏览器关闭时应清空此cookie，但C#会报错
                {
                    if (match.Groups[3].Value == "=")
                    {
                        return match.Value.Substring(match.Value.LastIndexOf(","));
                    }
                    return string.Empty;
                }

                if (expiresValue.Contains(","))
                {
                    if (!DateTime.TryParse(expiresValue, out DateTime dateTime))
                    {
                        if (match.Groups[3].Value == "=")
                        {
                            return match.Value.Substring(match.Value.LastIndexOf(","));
                        }
                        return string.Empty;
                    }
                    string utcFormat = dateTime.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                    if (match.Groups[3].Value == "=")
                    {
                        return $"Expires={utcFormat}{match.Value.Substring(match.Value.LastIndexOf(","))}";
                    }
                    else
                    {
                        return $"Expires={utcFormat}{match.Groups[3].Value}";
                    }
                }
                else return match.Value;
            });
        }
    }
}