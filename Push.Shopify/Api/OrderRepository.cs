using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Http;
using Push.Foundation.Web.Misc;


namespace Push.Shopify.Api
{

    public class OrderRepository
    {
        private readonly HttpFacade _executionFacade;
        private readonly IPushLogger _logger;

        public OrderRepository(
                HttpFacade executionFacade, IPushLogger logger)
        {
            _executionFacade = executionFacade;            
            _logger = logger;
        }



        //public virtual int RetrieveCount(OrderFilter filter)
        //{
        //    var url = "/admin/orders/count.json?" + filter.ToQueryStringBuilder();
        //    var request = _requestFactory.HttpGet(url);
        //    var clientResponse = _executionFacade.ExecuteRequestWithInsistence(request);

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
        //    var clientResponse = _executionFacade.ExecuteRequestWithInsistence(request);
        //    return clientResponse.Body;
        //}

            
        
        public virtual string Retrieve(long orderId)
        {
            var path = $"/admin/orders/{orderId}.json";
            var response = _executionFacade.Get(path);
            return response.Body;
        }

        public virtual string RetrieveTransactions(long orderId)
        {
            var path = $"/admin/orders/{orderId}/transactions.json";
            var response = _executionFacade.Get(path);

            return response.Body;
        }

        
        public void Insert(string orderJson)
        {
            var path = "/admin/orders.json";
            var clientResponse = _executionFacade.Get(path);
        }

    }
}
