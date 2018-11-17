using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Shopify.Http;


namespace Push.Shopify.Api
{

    public class OrderApi
    {
        private readonly ShopifyHttpContext _httpClient;
        private readonly IPushLogger _logger;

        public OrderApi(
                    IPushLogger logger, ShopifyHttpContext httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        
        public virtual string Retrieve(SearchFilter filter)
        {
            var querystring = filter.ToQueryString();
            var path = string.Format("/admin/orders.json?" + querystring);
            var response = _httpClient.Get(path);
            return response.Body;
        }
        
        public virtual string Retrieve(long orderId)
        {
            var path = $"/admin/orders/{orderId}.json";
            var response = _httpClient.Get(path);
            return response.Body;
        }
        
        public virtual string RetrieveByName(long orderName)
        {
            var path = $"/admin/orders.json?name={orderName}";
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public virtual string RetrieveTransactions(long orderId)
        {
            var path = $"/admin/orders/{orderId}/transactions.json";
            var response = _httpClient.Get(path);

            return response.Body;
        }

        
        
        public void Insert(string orderJson)
        {
            var path = "/admin/orders.json";
            var clientResponse = _httpClient.Get(path);
        }

    }
}
