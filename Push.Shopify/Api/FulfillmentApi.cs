using Push.Foundation.Utilities.Logging;
using Push.Shopify.Http;


namespace Push.Shopify.Api
{

    public class FulfillmentApi
    {
        private readonly ShopifyHttpContext _httpClient;
        private readonly IPushLogger _logger;

        public FulfillmentApi(
                    IPushLogger logger, ShopifyHttpContext httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        

        public string Insert(long orderId, string fulfillmentJson)
        {
            var path = $"/admin/orders/{orderId}/fulfillments.json";
            var clientResponse = _httpClient.Post(path, fulfillmentJson);
            return clientResponse.Body;
        }

    }
}
