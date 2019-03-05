using System;
using Monster.Middle.Persist.Tenant;

namespace Monster.Middle.Processes.Sync.Model.Inventory
{
    public class VariantAndStockItemDto
    {
        public long MonsterVariantId { get; set; }
        public long ShopifyProductId { get; set; }
        public string ShopifyProductTitle { get; set; }
        public string ShopifyVendor { get; set; }
        public string ShopifyProductType { get; set; }
        public long ShopifyVariantId { get; set; }
        public string ShopifySku { get; set; }
        public string ShopifyVariantTitle { get; set; }
        public string AcumaticaItemId { get; set; }
        public string AcumaticaDescription { get; set; }
        public bool IsSyncEnabled { get; set; }

        public string ShopifyProductUrl { get; set; }
        public string ShopifyVariantUrl { get; set; }
        public string AcumaticaStockItemUrl { get; set; }

        public static VariantAndStockItemDto Make(
                        UsrShopAcuItemSync input, 
                        Func<long, long, string> variantUrlService,
                        Func<string, string> stockItemUrlService)
        {
            var output = new VariantAndStockItemDto();
            output.MonsterVariantId = input.UsrShopifyVariant.MonsterId;
            output.ShopifyProductId = input.UsrShopifyVariant.UsrShopifyProduct.ShopifyProductId;
            output.ShopifyProductTitle = input.UsrShopifyVariant.UsrShopifyProduct.ShopifyTitle;
            output.ShopifyProductType = input.UsrShopifyVariant.UsrShopifyProduct.ShopifyProductType;
            output.ShopifyVendor = input.UsrShopifyVariant.UsrShopifyProduct.ShopifyVendor;
            output.ShopifyVariantId = input.UsrShopifyVariant.ShopifyVariantId;
            output.ShopifySku = input.UsrShopifyVariant.ShopifySku;
            output.ShopifyVariantTitle = input.UsrShopifyVariant.ShopifyTitle;

            output.ShopifyVariantUrl 
                = variantUrlService(
                    input.UsrShopifyVariant.UsrShopifyProduct.ShopifyProductId,
                    input.UsrShopifyVariant.ShopifyVariantId);

            output.AcumaticaItemId = input.UsrAcumaticaStockItem.ItemId;
            output.AcumaticaDescription = input.UsrAcumaticaStockItem.AcumaticaDescription;

            output.AcumaticaStockItemUrl = stockItemUrlService(input.UsrAcumaticaStockItem.ItemId);

            output.IsSyncEnabled = input.IsSyncEnabled;

            return output;
        }
    }
}
