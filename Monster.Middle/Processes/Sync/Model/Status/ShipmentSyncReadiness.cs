using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class ShipmentSyncReadiness
    {
        public bool WarehouseLocationUnmatched { get; set; }
        public List<string> UnmatchedVariantStockItems { get; set; }
        public List<string> StockItemsWithInsuffientInventory { get; set; }
        

        public bool IsReady
            => !WarehouseLocationUnmatched
               && UnmatchedVariantStockItems.Count == 0
               && StockItemsWithInsuffientInventory.Count == 0;
               

        public ShipmentSyncReadiness()
        {
            WarehouseLocationUnmatched = false;
            StockItemsWithInsuffientInventory = new List<string>();
            UnmatchedVariantStockItems = new List<string>();
        }
    }
}

