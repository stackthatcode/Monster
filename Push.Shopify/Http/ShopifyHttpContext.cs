﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Push.Foundation.Utilities.Execution;
using Push.Foundation.Utilities.Http;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Config;
using Push.Shopify.Http.Credentials;


namespace Push.Shopify.Http
{
    public class ShopifyHttpContext : IDisposable
    {
        private readonly ShopifyHttpConfig _httpConfig;
        private readonly IPushLogger _logger;

        // Hydrated by calls to Initialize()
        private HttpClient _httpClient;
        private FaultTolerantExecutor _executor;
        public Uri BaseAddress { private set; get; }


        public ShopifyHttpContext(ShopifyHttpConfig httpConfig, IPushLogger logger)
        {
            _httpConfig = httpConfig;
            _logger = logger;
        }
        
        public void Initialize(IShopifyCredentials credentials, int? shopifyDelayMsOverride = null)
        {
            BaseAddress = new Uri(credentials.Domain.BaseUrl);

            var throttlingDelay = shopifyDelayMsOverride ?? _httpConfig.ThrottlingDelay;

            _executor = new FaultTolerantExecutor()
            {
                MaxNumberOfAttempts = _httpConfig.MaxAttempts,
                ThrottlingKey = credentials.Domain.BaseUrl,
                ThrottlingDelay = throttlingDelay,
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
                        BaseAddress = this.BaseAddress,
                        DefaultRequestHeaders =
                        {
                            Accept =
                            {
                                MediaTypeWithQualityHeaderValue.Parse("application/json")
                            }
                        }
                };

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Spawn the WebRequest
            //
            _httpClient.Timeout = new TimeSpan(0, 0, 0, _httpConfig.Timeout);

            // Key and Secret authentication, for purpose of retrieving OAuth access token
            //
            if (credentials is ApiKeyAndSecret)
            {
                var oAuthKeyAndSecret = credentials as ApiKeyAndSecret;

                var basicCreds = $"{oAuthKeyAndSecret.ApiKey}:{oAuthKeyAndSecret.ApiSecret}";

                _httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Basic", basicCreds);
            }

            // Authentication using OAuth access token
            //
            if (credentials is OAuthAccessToken)
            {
                var accessTokenCred = credentials as OAuthAccessToken;

                _httpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", accessTokenCred.AccessToken);
            }

            // Authentication using Key Credentials i.e. Shopify private app
            //
            if (credentials is PrivateAppCredentials)
            {
                var privateAppCreds = credentials as PrivateAppCredentials;

                var headerValue = $"{privateAppCreds.ApiKey}:{privateAppCreds.ApiPassword}";
                var byteArray = Encoding.ASCII.GetBytes(headerValue);

                _httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
        }


        public ResponseEnvelope Get(string url, Dictionary<string, string> headers = null)
        {
            var urlDebug = $"HTTP GET on {url}";
            var errorContext = BuildErrorContext(urlDebug);
            _logger.Debug(urlDebug);

            // Act
            var response = _executor.Do(() => _httpClient.GetAsync(url).Result, errorContext);
            
            // Process
            var output = response.ToEnvelope();
            _logger.Trace(output.Body);
            ProcessStatusCodes(output, errorContext);

            return output;
        }

        public ResponseEnvelope Post(string url, string content)
        {
            var urlDebug = $"HTTP POST on {url}";
            var errorContext = BuildErrorContext(urlDebug, content);

            _logger.Debug(urlDebug);
            _logger.Trace(content);

            // Warning - change this at your own risk
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

            // Act
            var response 
                = _executor.Do(() => _httpClient.PostAsync(url, httpContent).Result, errorContext);

            // Process
            var output = response.ToEnvelope();
            _logger.Trace(output.Body);
            ProcessStatusCodes(output, errorContext);

            return output;
        }

        public ResponseEnvelope Put(string url, string content)
        {
            var urlDebug = $"HTTP PUT on {url}";
            var errorContext = BuildErrorContext(urlDebug, content);

            _logger.Debug(urlDebug);
            _logger.Trace(content);

            // Warning - change this at your own risk
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

            // Act
            var response = 
                _executor.Do(() => _httpClient.PutAsync(url, httpContent).Result, errorContext);

            // Process
            var output = response.ToEnvelope();
            _logger.Trace(output.Body);
            ProcessStatusCodes(output, errorContext);
            return output;
        }

        private string BuildErrorContext(string url, string requestBody = null)
        {
            var output = $"*** Failed HTTP Request: {url}";

            if (requestBody != null)
            {
                output += Environment.NewLine + requestBody;
            }

            return output;
        }

        public ResponseEnvelope ProcessStatusCodes(ResponseEnvelope response, string errorContext)
        {
            // All other non-200 calls throw an exception
            if (response.HasBadStatusCode)
            {
                throw new Exception(
                    $"Bad Status Code - HTTP {(int)response.StatusCode} ({response.StatusCode})"
                    + Environment.NewLine + errorContext);
            }

            return response;
        }


        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}

