using System;
using Monster.Middle.Persist.Instance;

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
                        ShopAcuItemSync input, 
                        Func<long, long, string> variantUrlService,
                        Func<string, string> stockItemUrlService)
        {
            var output = new VariantAndStockItemDto();

            output.MonsterVariantId = input.ShopifyVariant.MonsterId;
            output.ShopifyProductId = input.ShopifyVariant.ShopifyProduct.ShopifyProductId;
            output.ShopifyProductTitle = input.ShopifyVariant.ShopifyProduct.ShopifyTitle;
            output.ShopifyProductType = input.ShopifyVariant.ShopifyProduct.ShopifyProductType;
            output.ShopifyVendor = input.ShopifyVariant.ShopifyProduct.ShopifyVendor;
            output.ShopifyVariantId = input.ShopifyVariant.ShopifyVariantId;
            output.ShopifySku = input.ShopifyVariant.ShopifySku;
            output.ShopifyVariantTitle = input.ShopifyVariant.ShopifyTitle;

            output.ShopifyVariantUrl 
                = variantUrlService(
                        input.ShopifyVariant.ShopifyProduct.ShopifyProductId,
                        input.ShopifyVariant.ShopifyVariantId);

            output.AcumaticaItemId = input.AcumaticaStockItem.ItemId;
            output.AcumaticaDescription = input.AcumaticaStockItem.AcumaticaDescription;

            output.AcumaticaStockItemUrl = stockItemUrlService(input.AcumaticaStockItem.ItemId);

            output.IsSyncEnabled = input.IsSyncEnabled;

            return output;
        }
    }
}
