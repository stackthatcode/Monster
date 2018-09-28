using Push.Foundation.Utilities.Logging;
using Push.Shopify.Http;

namespace Push.Shopify.Api
{
    public class InventoryApi
    {
        private readonly ShopifyHttpContext _httpClient;
        private readonly IPushLogger _logger;

        public InventoryApi(IPushLogger logger, ShopifyHttpContext httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public string RetrieveLocations()
        {
            var path = $"/admin/locations.json";
            var response = _httpClient.Get(path);
            return response.Body;
        }
    }
}
