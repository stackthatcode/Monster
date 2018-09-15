using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Push.Shopify.Config;
using Push.Shopify.Http.Credentials;

namespace Push.Shopify.Http
{
    public class ShopifyHttpClientFactory
    {
        private readonly ShopifyClientSettings _config;

        public ShopifyHttpClientFactory(ShopifyClientSettings config)
        {
            _config = config;
        }
        
        public HttpClient Make(IShopifyCredentials credentials)
        {
            var baseAddress = new Uri(credentials.Domain.BaseUrl);
            var httpClient 
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
                                MediaTypeWithQualityHeaderValue.Parse("text/json")
                            }
                        }
                    };

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Spawn the WebRequest
            httpClient.Timeout = new TimeSpan(0, 0, 0, _config.Timeout);

            // Key and Secret authentication, for purpose of retrieving OAuth access token
            if (credentials is ApiKeyAndSecret)
            {
                var oAuthKeyAndSecret = credentials as ApiKeyAndSecret;

                var basicCreds = 
                    $"{oAuthKeyAndSecret.ApiKey}:{oAuthKeyAndSecret.ApiSecret}";

                httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Basic", basicCreds);
            }

            // Authentication using OAuth access token
            if (credentials is OAuthAccessToken)
            {
                var accessTokenCred 
                        = credentials as OAuthAccessToken;

                httpClient.DefaultRequestHeaders.Add(
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

                httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Basic", headerValue);
            }
            
            return httpClient;
        }
    }
}

