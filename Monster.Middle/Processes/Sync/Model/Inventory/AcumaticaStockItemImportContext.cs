using System.Collections.Generic;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Inventory
{
    public class AcumaticaStockItemImportContext
    {
        public List<long> ShopifyProductIds { get; set; }
        public bool CreateInventoryReceipts { get; set; }
        public string WarehouseId { get; set; }
        public bool IsSyncEnabled { get; set; }


        // Added as a result of auto-matches
        //
        public List<ShopifyVariant> VariantsForInventoryReceipt { get; set; }

        public AcumaticaStockItemImportContext()
        {
            ShopifyProductIds = new List<long>();
            VariantsForInventoryReceipt = new List<ShopifyVariant>();
        }
    }
}

