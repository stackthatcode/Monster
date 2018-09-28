using Push.Shopify.Http;

namespace Push.Shopify.Api
{
    public class ShopRepository
    {
        private readonly ShopifyHttpContext _httpClient;
        
        public ShopRepository(ShopifyHttpContext httpClient)
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

