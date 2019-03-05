using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public class ShipmentSyncReadiness
    {
        public bool AnyShopifyMadeFulfillments { get; set; }
        public bool WarehouseLocationUnmatched { get; set; }
        public List<string> UnmatchedVariantStockItems { get; set; }
        public List<string> StockItemsWithInsuffientInventory { get; set; }
        public List<string> VariantsWithInsuffientInventory { get; set; }


        public bool IsReady
            => !AnyShopifyMadeFulfillments
               && !WarehouseLocationUnmatched
               && UnmatchedVariantStockItems.Count == 0
               && StockItemsWithInsuffientInventory.Count == 0
               && VariantsWithInsuffientInventory.Count == 0;


        public ShipmentSyncReadiness()
        {
            WarehouseLocationUnmatched = false;
            StockItemsWithInsuffientInventory = new List<string>();
            VariantsWithInsuffientInventory = new List<string>();
            UnmatchedVariantStockItems = new List<string>();
        }
    }
}

