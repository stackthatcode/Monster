using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Processes.Shopify.Persist;

namespace Monster.Web.Models.RealTime
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
        

        public static ShopifyProductModel Make(
                UsrShopifyProduct input, 
                Func<long, string> productUrlBuilder,
                bool includeVariantGraph = false)
        {
            var output = new ShopifyProductModel();

            output.ProductTitle = input.ShopifyTitle;
            output.ProductType = input.ShopifyProductType;
            output.Vendor = input.ShopifyVendor;
            output.ShopifyProductId = input.ShopifyProductId;
            output.VariantCount = input.UsrShopifyVariants.Count();
            output.ShopifyUrl = productUrlBuilder(input.ShopifyProductId);

            output.SyncedVariantCount
                = input.UsrShopifyVariants
                    .Count(x => x.UsrShopAcuItemSyncs != null 
                                && x.UsrShopAcuItemSyncs.Count > 0);

            if (includeVariantGraph)
            {
                output.Variants
                    = input.UsrShopifyVariants
                        .Select(x => ShopifyVariantModel.Make(x))
                        .ToList();
            }

            return output;
        }
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

        public static ShopifyVariantModel Make(UsrShopifyVariant input)
        {
            var output = new ShopifyVariantModel();
            output.ShopifyVariantId = input.ShopifyVariantId;
            output.Sku = input.ShopifySku;
            output.VariantTitle = input.ShopifyTitle;
            output.Price = (decimal)input.ToVariantObj().price;
            output.AvailableQuantity 
                = input.UsrShopifyInventoryLevels.Sum(x => x.ShopifyAvailableQuantity);

            output.IsLoadedInAcumatica = input.IsMatched();

            return output;
        }
    }
}