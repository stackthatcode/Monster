﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Push.Foundation.Utilities.Http;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Shopify.Config;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;

namespace Push.Shopify.Api
{
    public class OAuthApi
    {
        private readonly ShopifyHttpContext _httpClient;
        private readonly IPushLogger _logger;

        public OAuthApi(
            IPushLogger logger, 
            ShopifyHttpContext httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }


        public string RetrieveAccessToken(string code, ApiKeyAndSecret credentials)
        {
            var queryString
                = new QueryStringBuilder()
                    .Add("client_id", credentials.ApiKey)
                    .Add("client_secret", credentials.ApiSecret)
                    .Add("code", code)
                    .ToString();

            var url = $"/admin/oauth/access_token?{queryString}";
            var response = _httpClient.Post(url, "");
                         
            dynamic parent = JsonConvert.DeserializeObject(response.Body);
            return parent.access_token;
        }
    }
}

