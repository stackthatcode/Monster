using Push.Shopify.Http;

namespace Push.Shopify.Api
{
    public class ShopApi
    {
        private readonly ShopifyHttpContext _httpClient;
        
        public ShopApi(ShopifyHttpContext httpClient)
        {
            _httpClient = httpClient;
        }

        public virtual string RetrieveShops()
        {
            var path = "/admin/shop.json";                       
            var clientResponse = _httpClient.Get(path);

            var output = clientResponse.Body;
            return output;
        }

        public virtual string RetrieveCarriers()
        {
            var path = "/admin/carrier_services.json";
            var clientResponse = _httpClient.Get(path);

            var output = clientResponse.Body;
            return output;
        }
    }
}

