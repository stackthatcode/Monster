using System;
using System.Collections.Generic;

namespace Push.Shopify.Api.Inventory
{

    public class InventoryItem
    {
        public long id { get; set; }
        public decimal? cost { get; set; }
        public bool tracked { get; set; }
    }

    public class InventoryItemUpdate
    {
        public long id { get; set; }
        public decimal cost { get; set; }
    }


    public class InventoryItemList
    {
        public List<InventoryItem> inventory_items { get; set; }
    }
}
