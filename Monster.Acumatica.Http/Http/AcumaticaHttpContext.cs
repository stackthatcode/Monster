using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Monster.Acumatica.Config;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Http;
using Push.Foundation.Web.Misc;

namespace Monster.Acumatica.Http
{
    public class AcumaticaHttpContext : IDisposable
    {
        public readonly Guid Identifier = Guid.NewGuid();

        private readonly AcumaticaHttpConfig _settings;
        private readonly IPushLogger _logger;

        // These are set by the Initialize method
        private string _instanceUrl;
        private HttpClient _httpClient;
        private ExecutorContext _executorContext;

        public AcumaticaHttpContext(
                AcumaticaHttpConfig settings, 
                IPushLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }
        
        public void Initialize(AcumaticaCredentials credentials)
        {
            var baseAddress = new Uri(credentials.InstanceUrl);
            _instanceUrl = credentials.InstanceUrl;

            _executorContext = new ExecutorContext()
            {
                NumberOfAttempts = _settings.RetryLimit,
                ThrottlingKey = credentials.InstanceUrl,
                Logger = _logger,
            };

            _httpClient 
                = new HttpClient(
                    new HttpClientHandler
                    {
                        UseCookies = true,
                        CookieContainer = new CookieContainer(),
                    })
                    {
                        BaseAddress = baseAddress,
                        DefaultRequestHeaders =
                        {
                            Accept =
                            {
                                MediaTypeWithQualityHeaderValue.Parse("text/json")
                            }
                        }
                    };

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Spawn the WebRequest
            _httpClient.Timeout = new TimeSpan(0, 0, 0, _settings.Timeout);
        }


        public ResponseEnvelope Get(
            string url,
            Dictionary<string, string> headers = null)
        {
            _logger.Debug($"HTTP GET on {url}");

            var response =
                DurableExecutor.Do(
                    () => _httpClient.GetAsync(url).Result, _executorContext);

            return response
                .ToEnvelope()
                .ProcessStatusCodes();
        }

        public ResponseEnvelope Post(string url, string content)
        {
            _logger.Debug($"HTTP POST on {url}");

            var httpContent
                = new StringContent(content, Encoding.UTF8, "application/json");

            var response =
                DurableExecutor.Do(
                    () => _httpClient.PostAsync(url, httpContent).Result,
                    _executorContext);

            return response
                .ToEnvelope()
                .ProcessStatusCodes();
        }

        public ResponseEnvelope Put(string url, string content)
        {
            _logger.Debug($"HTTP PUT on {url}");

            var httpContent
                = new StringContent(content, Encoding.UTF8, "application/json");

            var response =
                DurableExecutor.Do(
                    () => _httpClient.PutAsync(url, httpContent).Result,
                    _executorContext);

            return response
                .ToEnvelope()
                .ProcessStatusCodes();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}

