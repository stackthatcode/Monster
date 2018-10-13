﻿using Push.Shopify.Http;

namespace Push.Shopify.Api
{
    public class ShopApi
    {
        private readonly ShopifyHttpContext _httpClient;
        
        public ShopApi(ShopifyHttpContext httpClient)
        {
            _httpClient = httpClient;
        }

        public virtual string Retrieve()
        {
            var path = "/admin/shop.json";                       
            var clientResponse = _httpClient.Get(path);

            var output = clientResponse.Body;
            return output;
        }        
    }
}
