using System.Collections.Generic;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Inventory
{
    public class AcumaticaStockItemImportContext
    {
        // Parameter fields
        //
        public List<long> ShopifyProductIds { get; set; }
        public bool CreateInventoryReceipts { get; set; }
        public string WarehouseId { get; set; }
        public bool IsSyncEnabled { get; set; }
        public bool SynchronizeOnly { get; set; }


        // Added as a result of auto-matches
        //
        public List<ShopifyVariant> VariantsForNextInventoryReceipt { get; set; }


        public AcumaticaStockItemImportContext()
        {
            ShopifyProductIds = new List<long>();

            // A downstream container for Variants to be loaded into an Inventory Receipt
            //
            VariantsForNextInventoryReceipt = new List<ShopifyVariant>();
        }
    }
}

