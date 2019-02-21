using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        public string ShopifyUrl { get; set; }


        public static FilterInventoryModel Make(
                UsrShopifyProduct input, 
                Func<long, string> urlBuilder)
        {
            var output = new FilterInventoryModel();

            output.ProductTitle = input.ShopifyTitle;
            output.ProductType = input.ShopifyProductType;
            output.Vendor = input.ShopifyVendor;
            output.ShopifyProductId = input.ShopifyProductId;
            output.VariantCount = input.UsrShopifyVariants.Count();
            output.ShopifyUrl = urlBuilder(input.ShopifyProductId);

            output.SyncedVariantCount
                = input.UsrShopifyVariants
                    .Count(x => x.UsrShopAcuItemSyncs != null 
                                && x.UsrShopAcuItemSyncs.Count > 0);

            return output;
        }
    }
}