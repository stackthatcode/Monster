using Push.Shopify.Http;


namespace Push.Shopify.Api
{

    public class OrderApi
    {
        private readonly ShopifyHttpContext _httpClient;

        public OrderApi(ShopifyHttpContext httpClient)
        {
            _httpClient = httpClient;
        }

        
        public string Retrieve(SearchFilter filter)
        {
            var querystring = filter.ToQueryString();
            var path = string.Format("/admin/orders.json?" + querystring);
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public string OrderInterfaceUrlById(long orderId)
        {
            return $"{_httpClient.BaseAddress}admin/orders/{orderId}";
        }
        
        public string Retrieve(long orderId)
        {
            var path = $"/admin/orders/{orderId}.json";
            var response = _httpClient.Get(path);
            return response.Body;
        }
        
        public string RetrieveByName(string orderName)
        {
            var path = $"/admin/orders.json?name={orderName}";
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public string RetrieveByNumber(long orderNumber)
        {
            var path = $"/admin/orders.json?order_number={orderNumber}";
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public string RetrieveCount()
        {
            var path = $"/admin/orders/count.json";
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public string RetrieveTransactions(long orderId)
        {
            var path = $"/admin/orders/{orderId}/transactions.json";
            var response = _httpClient.Get(path);

            return response.Body;
        }
        
        public string Insert(string json)
        {
            var path = "/admin/orders.json";
            var response = _httpClient.Post(path, json);
            return response.Body;
        }

        public string InsertTransaction(long order_id, string json)
        {
            var path = $"/admin/orders/{order_id}/transactions.json";
            var response = _httpClient.Post(path, json);
            return response.Body;
        }
    }
}

