using Push.Foundation.Utilities.Logging;
using Push.Shopify.Http;


namespace Push.Shopify.Api
{
    public class CustomerApi
    {
        private readonly ShopifyHttpContext _httpClient;
        private readonly IPushLogger _logger;

        public CustomerApi(IPushLogger logger, ShopifyHttpContext httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        
        public string Retrieve(long customerId)
        {
            var path = $"/admin/customers/{customerId}.json";
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public string Retrieve(SearchFilter filter)
        {
            var path = $"/admin/customers.json?{filter.ToQueryString()}";
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public string Search(string query)
        {
            var path = $"/admin/customers/search.json?query={query}";
            var response = _httpClient.Get(path);
            return response.Body;

        }

        public string Create(string json)
        {
            var path = $"/admin/customers.json";
            var response = _httpClient.Post(path,json);
            return response.Body;
        }
    }
}

