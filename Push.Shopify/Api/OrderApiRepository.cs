using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.HttpClient;
using Push.Shopify.HttpClient;


namespace Push.Shopify.Api
{

    public class OrderApiRepository
    {
        private readonly ShopifyRequestBuilder _requestFactory;
        private readonly ClientFacade _executionFacade;
        private readonly IPushLogger _logger;

        public OrderApiRepository(
                ClientFacade executionFacade,
                ShopifyRequestBuilder requestFactory,
                IPushLogger logger)
        {
            _executionFacade = executionFacade;            
            _requestFactory = requestFactory;
            _logger = logger;
        }



        //public virtual int RetrieveCount(OrderFilter filter)
        //{
        //    var url = "/admin/orders/count.json?" + filter.ToQueryStringBuilder();
        //    var request = _requestFactory.HttpGet(url);
        //    var clientResponse = _executionFacade.ExecuteRequest(request);

        //    dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
        //    var count = parent.count;
        //    return count;
        //}

        //public virtual string Retrieve(
        //            OrderFilter filter, int page = 1, int limit = 250)
        //{
        //    var querystring
        //        = new QueryStringBuilder()
        //            .Add("page", page)
        //            .Add("limit", limit)
        //            .Add(filter.ToQueryStringBuilder())
        //            .ToString();

        //    var path = string.Format("/admin/orders.json?" + querystring);

        //    var request = _requestFactory.HttpGet(path);
        //    var clientResponse = _executionFacade.ExecuteRequest(request);
        //    return clientResponse.Body;
        //}

            
        
        public virtual string Retrieve(long orderId)
        {
            var path = $"/admin/orders/{orderId}.json";
            var request = _requestFactory.HttpGet(path);
            var response = _executionFacade.ExecuteRequest(request);

            return response.Body;
        }

        public virtual string RetrieveTransactions(long orderId)
        {
            var path = $"/admin/orders/{orderId}/transactions.json";
            var request = _requestFactory.HttpGet(path);
            var response = _executionFacade.ExecuteRequest(request);

            return response.Body;
        }

        
        public void Insert(string orderJson)
        {
            var path = "/admin/orders.json";
            var request = _requestFactory.HttpPost(path, orderJson);
            var clientResponse = _executionFacade.ExecuteRequest(request);
        }

    }
}
