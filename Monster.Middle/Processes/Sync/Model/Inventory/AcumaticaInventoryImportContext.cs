using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Model.Inventory
{
    public class AcumaticaInventoryImportContext
    {
        public List<long> ShopifyProductIds { get; set; }
        public bool CreateInventoryReceipts { get; set; }
        public bool IsSyncEnabled { get; set; }
    }
}
