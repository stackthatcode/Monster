using Push.Foundation.Utilities.Logging;
using Push.Shopify.Http;


namespace Push.Shopify.Api
{
    public class CustomerApi
    {
        private readonly ShopifyHttpContext _httpClient;
        private readonly IPushLogger _logger;

        public CustomerApi(
                    IPushLogger logger, 
                    ShopifyHttpContext httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        
        public virtual string Retrieve(long customerId)
        {
            var path = $"/admin/customers/{customerId}.json";
            var response = _httpClient.Get(path);
            return response.Body;
        }
    }
}
