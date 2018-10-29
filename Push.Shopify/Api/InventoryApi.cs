using System.Collections.Generic;
using Push.Foundation.Utilities.General;
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

        public string RetrieveInventoryLevels(IList<long> itemIds)
        {
            var path = 
                $"/admin/inventory_levels.json?" +
                $"inventory_item_ids={itemIds.ToCommaSeparatedList()}";
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public string SetInventoryLevels(string content)
        {
            var path = "/admin/inventory_levels/set.json";            
            var response = _httpClient.Post(path, content);
            return response.Body;
        }
    }
}
