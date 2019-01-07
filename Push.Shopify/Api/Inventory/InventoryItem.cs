using System;
using System.Collections.Generic;

namespace Push.Shopify.Api.Inventory
{

    public class InventoryItem
    {
        public long id { get; set; }
        public string sku { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public decimal? cost { get; set; }
        public bool tracked { get; set; }
        public string admin_graphql_api_id { get; set; }
    }

    public class InventoryItemList
    {
        public List<InventoryItem> inventory_items { get; set; }
    }
}
