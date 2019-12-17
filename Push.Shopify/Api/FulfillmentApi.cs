using Push.Shopify.Http;


namespace Push.Shopify.Api
{

    public class FulfillmentApi
    {
        private readonly ShopifyHttpContext _httpClient;

        public FulfillmentApi(ShopifyHttpContext httpClient)
        {
            _httpClient = httpClient;
        }

        public string Insert(long orderId, string fulfillmentJson)
        {
            var path = $"/admin/orders/{orderId}/fulfillments.json";
            var clientResponse = _httpClient.Post(path, fulfillmentJson);
            return clientResponse.Body;
        }

        public string Update(long orderId, long fulfillmentId, string fulfillmentJson)
        {
            var path = $"/admin/orders/{orderId}/fulfillments/{fulfillmentId}.json";
            var clientResponse = _httpClient.Put(path, fulfillmentJson);
            return clientResponse.Body;
        }
    }
}
