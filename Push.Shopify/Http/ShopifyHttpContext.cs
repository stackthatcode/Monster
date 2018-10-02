using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Http;
using Push.Foundation.Web.Misc;
using Push.Shopify.Config;
using Push.Shopify.Http.Credentials;

namespace Push.Shopify.Http
{
    public class ShopifyHttpContext : IDisposable
    {
        private readonly ShopifyHttpConfig _settings;
        private readonly IPushLogger _logger;

        // Hydrated by calls to Initialize()
        private HttpClient _httpClient;
        private ExecutorContext _executorContext;


        public ShopifyHttpContext(
                ShopifyHttpConfig settings, 
                IPushLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }
        
        public void Initialize(IShopifyCredentials credentials)
        {
            var baseAddress = new Uri(credentials.Domain.BaseUrl);
            
            _executorContext = new ExecutorContext()
            {
                NumberOfAttempts = _settings.RetryLimit,
                ThrottlingKey = credentials.Domain.BaseUrl,
                Logger = _logger,
            };

            _httpClient
                = new HttpClient(
                    new HttpClientHandler
                    {
                        UseCookies = true,
                        CookieContainer = new CookieContainer()
                    })
                    {
                        BaseAddress = baseAddress,
                        DefaultRequestHeaders =
                        {
                            Accept =
                            {
                                MediaTypeWithQualityHeaderValue
                                    .Parse("application/json")
                            }
                        }
                };

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Spawn the WebRequest
            _httpClient.Timeout = new TimeSpan(0, 0, 0, _settings.Timeout);

            // Key and Secret authentication, for purpose of retrieving OAuth access token
            if (credentials is ApiKeyAndSecret)
            {
                var oAuthKeyAndSecret = credentials as ApiKeyAndSecret;

                var basicCreds = 
                    $"{oAuthKeyAndSecret.ApiKey}:{oAuthKeyAndSecret.ApiSecret}";

                _httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Basic", basicCreds);
            }

            // Authentication using OAuth access token
            if (credentials is OAuthAccessToken)
            {
                var accessTokenCred 
                        = credentials as OAuthAccessToken;

                _httpClient.DefaultRequestHeaders.Add(
                    "X-Shopify-Access-Token", 
                    accessTokenCred.AccessToken);
            }

            // Authentication using Key Credentials i.e. Shopify private app
            if (credentials is PrivateAppCredentials)
            {
                var privateAppCredentials 
                        = credentials as PrivateAppCredentials;

                var headerValue
                    = $"{privateAppCredentials.ApiKey}:{privateAppCredentials.ApiPassword}";
                var byteArray = Encoding.ASCII.GetBytes(headerValue);

                _httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
        }

        public ResponseEnvelope Get(
                string url,
                Dictionary<string, string> headers = null)
        {
            _logger.Debug($"HTTP GET on {url}");

            var response =
                DurableExecutor.Do(
                    () => _httpClient.GetAsync(url).Result, _executorContext);

            var output = response.ToEnvelope();
            _logger.Trace(output.Body);
            output.ProcessStatusCodes();
            return output;
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

            var output = response.ToEnvelope();
            _logger.Trace(output.Body);
            output.ProcessStatusCodes();
            return output;
        }

        public ResponseEnvelope Put(string url, string content)
        {
            _logger.Debug($"HTTP PUT on {url}");

            var httpContent
                = new StringContent(content, Encoding.UTF8, "text/json");

            var response =
                DurableExecutor.Do(
                    () => _httpClient.PutAsync(url, httpContent).Result, 
                            _executorContext);

            var output = response.ToEnvelope();
            _logger.Trace(output.Body);
            output.ProcessStatusCodes();
            return output;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

    }
}

