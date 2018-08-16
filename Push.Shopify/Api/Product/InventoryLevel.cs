using System;
using System.Collections.Generic;

namespace Push.Shopify.Api.Product
{
    public class InventoryLevel
    {
        public long inventory_item_id { get; set; }
        public long location_id { get; set; }
        public int available { get; set; }
        public DateTimeOffset updated_at { get; set; }
        public string admin_graphql_api_id { get; set; }
    }

    public class InventoryLevelList
    {
        public List<InventoryLevel> inventory_levels { get; set; }
    }
}
