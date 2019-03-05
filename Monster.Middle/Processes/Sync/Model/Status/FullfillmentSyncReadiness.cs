using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class FulfillmentSyncReadiness
    {
        public bool AnyShopifyMadeFulfillments { get; set; }
        public bool WarehouseLocationUnmatched { get; set; }
        public List<string> UnmatchedVariantStockItems { get; set; }
        public List<string> VariantsWithInsuffientInventory { get; set; }


        public bool IsReady
            => !AnyShopifyMadeFulfillments
               && !WarehouseLocationUnmatched
               && UnmatchedVariantStockItems.Count == 0
               && VariantsWithInsuffientInventory.Count == 0;


        public FulfillmentSyncReadiness()
        {
            AnyShopifyMadeFulfillments = false;
            WarehouseLocationUnmatched = false;
            VariantsWithInsuffientInventory = new List<string>();
            UnmatchedVariantStockItems = new List<string>();
        }
    }
}

