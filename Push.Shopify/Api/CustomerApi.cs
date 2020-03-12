using Push.Foundation.Utilities.Http;
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
            var path = $"/admin/api/2019-10/customers/{customerId}.json";
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public ResponseEnvelope Retrieve(SearchFilter filter)
        {
            var path = $"/admin/api/2019-10/customers.json?{filter.ToQueryString()}";
            var response = _httpClient.Get(path);
            return response;
        }

        public ResponseEnvelope Retrieve(string path)
        {
            var response = _httpClient.Get(path);
            return response;
        }

        public string Search(string query)
        {
            var path = $"/admin/api/2019-10/customers/search.json?query={query}";
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public string Create(string json)
        {
            var path = $"/admin/api/2019-10/customers.json";
            var response = _httpClient.Post(path,json);
            return response.Body;
        }
    }
}

