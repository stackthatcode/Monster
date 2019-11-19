using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Analysis
{
    public class ProductStockItemResultsRow
    {
        public long ShopifyProductId { get; set; }
        public string ShopifyProductTitle { get; set; }
        public string ShopifyProductUrl { get; set; }
        public long ShopifyVariantId { get; set; }
        public string ShopifyVariantTitle { get; set; }
        public string ShopifyVariantSku { get; set; }
        public string ShopifyVariantUrl { get; set; }
        public string ShopifyVariantTax { get; set; }
        public decimal ShopifyVariantPrice { get; set; }
        public int ShopifyVariantAvailQty { get; set; }
        public bool IsShopifyProductDeleted { get; set; }
        public bool IsShopifyVariantMissing { get; set; }

        public string AcumaticaItemId { get; set; }
        public string AcumaticaItemDesc { get; set; }
        public string AcumaticaItemUrl { get; set; }
        public string AcumaticaItemTax { get; set; }
        public decimal AcumaticaItemPrice { get; set; }
        public int AcumaticaItemAvailQty { get; set; }

        public bool HasMismatchedSku { get; set; }
        public bool HasMismatchedTaxes { get; set; }
        public bool HasDuplicateSkus { get; set; }
    }
}
