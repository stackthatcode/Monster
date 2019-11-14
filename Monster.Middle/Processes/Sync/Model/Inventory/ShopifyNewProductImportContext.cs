using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Model.Inventory
{
    public class ShopifyNewProductImportContext
    {
        public List<string> AcumaticaItemIds { get; set; }
        public string ProductTitle { get; set; }
        public string ProductType { get; set; }
        public string ProductVendor { get; set; }
    }
}
