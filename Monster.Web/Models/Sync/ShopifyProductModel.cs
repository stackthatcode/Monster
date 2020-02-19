using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;

namespace Monster.Web.Models.Sync
{
    public class ShopifyProductModel
    {
        public string ProductTitle { get; set; }
        public string ProductType { get; set; }
        public string Vendor { get; set; }
        public long ShopifyProductId { get; set; }
        public int VariantCount { get; set; } 
        public int SyncedVariantCount { get; set; }
        public int UnsyncedVariantCount => VariantCount - SyncedVariantCount;
        public string ShopifyUrl { get; set; }

        public List<ShopifyVariantModel> Variants { get; set; }
        
    }

    public class ShopifyVariantModel
    {
        public long ShopifyVariantId { get; set; }
        public decimal Price { get; set; }
        public string FormattedPrice => Price.ToString("C2");
        public string Sku { get; set; }
        public string VariantTitle { get; set; }
        public int AvailableQuantity { get; set; }
        public bool IsLoadedInAcumatica { get; set; }
        public bool IsMissing { get; set; }
    }
}