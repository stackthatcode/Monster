using System;
using System.IO;
using System.Net;
using System.Text;
using Push.Shopify.Config;
using Push.Shopify.HttpClient.Credentials;

namespace Push.Shopify.HttpClient
{
    public class ShopifyRequestBuilder
    {
        private readonly ShopifyClientSettings _config;

        public IShopifyCredentials Credentials { get; }


        // This is instanced by the ApiFactory, which passes the valid credentials
        public ShopifyRequestBuilder(ShopifyClientSettings config, IShopifyCredentials credentials)
        {
            _config = config;
            Credentials = credentials;
        }


        public HttpWebRequest HttpGet(string path)
        {
            var request = FactoryWorker(path);
            request.Method = "GET";
            return request;
        }

        public HttpWebRequest HttpPost(string path, string content)
        {
            var request = FactoryWorker(path);
            request.Method = "POST";

            var byteArray = Encoding.ASCII.GetBytes(content);
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            return request;
        }

        public HttpWebRequest HttpPut(string path, string content)
        {
            var request = FactoryWorker(path);
            request.Method = "PUT";

            var byteArray = Encoding.ASCII.GetBytes(content);
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            return request;
        }

        public HttpWebRequest HttpDelete(string path)
        {
            var request = FactoryWorker(path);
            request.Method = "DELETE";
            return request;
        }

        private HttpWebRequest FactoryWorker(string path)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Spawn the WebRequest
            var url = Credentials.Domain.BaseUrl + path;
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Timeout = _config.Timeout;
            
            // Key and Secret authentication, for purpose of retrieving OAuth access token
            if (Credentials is ApiKeyAndSecret)
            {
                var oAuthKeyAndSecret = Credentials as ApiKeyAndSecret;

                var credentialCache = new CredentialCache();
                credentialCache.Add(
                    new Uri(url), "Basic",
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
                    new Uri(url), "Basic",
                    new NetworkCredential(privateAppKeyAndPassword.ApiKey, privateAppKeyAndPassword.ApiPassword));

                req.Credentials = credentialCache;
            }


            return req;
        }
    }
}

