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


        public string RetrieveLocation(long id)
        {
            var path = $"/admin/api/2019-10/locations/{id}.json";
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public string RetrieveLocations()
        {
            var path = $"/admin/api/2019-10/locations.json";
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public string RetrieveInventoryLevels(IList<long> itemIds)
        {
            var path = $"/admin/api/2019-10/inventory_levels.json?inventory_item_ids={itemIds.ToCommaSeparatedList()}";
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public string RetrieveInventoryItems(IList<long> itemIds)
        {
            var path = $"/admin/api/2019-10/inventory_items.json?ids={itemIds.ToCommaSeparatedList()}";
            var response = _httpClient.Get(path);
            return response.Body;
        }

        public string SetInventoryLevels(string content)
        {
            var path = "/admin/api/2019-10/inventory_levels/set.json";            
            var response = _httpClient.Post(path, content);
            return response.Body;
        }

        public string SetInventoryCost(long inventory_item_id, string content)
        {
            var path = $"/admin/api/2019-10/inventory_items/{inventory_item_id}.json";
            var response = _httpClient.Put(path, content);
            return response.Body;
        }
    }
}
