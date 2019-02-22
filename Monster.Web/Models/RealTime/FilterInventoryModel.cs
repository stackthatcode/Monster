using System;
using System.Linq;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Web.Models.RealTime
{
    public class FilterInventoryModel
    {
        public string ProductTitle { get; set; }
        public string ProductType { get; set; }
        public string Vendor { get; set; }
        public long ShopifyProductId { get; set; }
        public int VariantCount { get; set; } 
        public int SyncedVariantCount { get; set; }
        public int UnsyncedVariantCount => VariantCount - SyncedVariantCount;
        public string ShopifyUrl { get; set; }




        public static FilterInventoryModel Make(
                UsrShopifyProduct input, 
                Func<long, string> productUrlBuilder,
                bool includeVariantGraph = false)
        {
            var output = new FilterInventoryModel();

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
                foreach (var variant in input.UsrShopifyVariants)
                {

                }
            }

            return output;
        }
    }

    public class FilterInventoryVariant
    {
        public long ShopifyVariantId { get; set; }
        public decimal Price { get; set; }
        public string FormattedPrice => Price.ToString("{0:C}");
        public string Sku { get; set; }
        public string Title { get; set; }
        public int AvailableQuantity { get; set; }
        public bool IsLoadedInAcumatica { get; set; }

        public static FilterInventoryVariant Make(UsrShopifyVariant input)
        {
            var output = new FilterInventoryVariant();
            output.ShopifyVariantId = input.ShopifyVariantId;
            //output.Price = input.p
        }
    }
}