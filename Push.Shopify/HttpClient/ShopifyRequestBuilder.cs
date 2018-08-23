using System;
using System.Net;
using Push.Foundation.Web.HttpClient;
using Push.Shopify.Config;
using Push.Shopify.HttpClient.Credentials;

namespace Push.Shopify.HttpClient
{
    public class ShopifyRequestBuilder : IRequestBuilder
    {
        private readonly ShopifyClientSettings _config;

        public IShopifyCredentials Credentials { get; }


        // This is instanced by the ApiFactory, which passes the valid credentials
        public  ShopifyRequestBuilder(ShopifyClientSettings config, IShopifyCredentials credentials)
        {
            _config = config;
            Credentials = credentials;
        }

        
        public HttpWebRequest Make(RequestEnvelope requestEnvelope)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Spawn the WebRequest
            var req = requestEnvelope.MakeWebRequest(Credentials.Domain.BaseUrl);
            req.Timeout = _config.Timeout;

            // Key and Secret authentication, for purpose of retrieving OAuth access token
            if (Credentials is ApiKeyAndSecret)
            {
                var oAuthKeyAndSecret = Credentials as ApiKeyAndSecret;

                var credentialCache = new CredentialCache();
                credentialCache.Add(
                    new Uri(Credentials.Domain.BaseUrl), "Basic",
                    new NetworkCredential(oAuthKeyAndSecret.ApiKey, oAuthKeyAndSecret.ApiSecret));

                req.Credentials = credentialCache;
            }

            // Authentication using OAuth access token
            if (Credentials is OAuthAccessToken)
            {
                var accessTokenCred = Credentials as OAuthAccessToken;
                req.Headers["X-Shopify-Access-Token"] = accessTokenCred.AccessToken;
            }

            // Authentication using Key Credentials i.e. Shopify private app
            if (Credentials is PrivateAppCredentials)
            {
                var privateAppKeyAndPassword = Credentials as PrivateAppCredentials;
                var credentialCache = new CredentialCache();
                credentialCache.Add(
                    new Uri(Credentials.Domain.BaseUrl), "Basic",
                    new NetworkCredential(privateAppKeyAndPassword.ApiKey, privateAppKeyAndPassword.ApiPassword));

                req.Credentials = credentialCache;
            }


            return req;
        }
    }
}

