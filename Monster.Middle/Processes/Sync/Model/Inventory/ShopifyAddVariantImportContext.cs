using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Model.Inventory
{
    public class ShopifyAddVariantImportContext
    {
        public long ShopifyProductId { get; set; }
        public List<string> AcumaticaItemIds { get; set; }
    }
}
