using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Push.Foundation.Utilities.Validation;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class InventorySyncStatus
    {
        public string StockItemId { get; set; }
        public bool IsStockItemPriceSynced { get; set; }
        public bool IsStockItemInventorySynced { get; set; }
        public long? ShopifyVariantId { get; set; }
        public string ShopifyVariantSku { get; set; }
        public bool IsShopifyVariantMissing { get; set; }


        // Computed
        public bool IsMatched => ShopifyVariantId.HasValue;
        public bool IsSkuMismatchedWithItemId
            => IsMatched && StockItemId.StandardizedSku() != ShopifyVariantSku.StandardizedSku();
        

        public static InventorySyncStatus Make(AcumaticaStockItem input)
        {
            var output = new InventorySyncStatus();
            output.StockItemId = input.ItemId;

            output.IsStockItemPriceSynced = input.IsPriceSynced;
            output.IsStockItemInventorySynced = input.AcumaticaWarehouseDetails.All(x => x.IsInventorySynced);

            if (input.IsSynced())
            {
                output.ShopifyVariantId = input.MatchedVariant().ShopifyVariantId;
                output.ShopifyVariantSku = input.MatchedVariant().ShopifySku;
                output.IsShopifyVariantMissing = input.MatchedVariant().IsMissing;
            }

            return output;
        }
        
        public ValidationResult ReadyToSyncPrice()
        {
            var validation = new Validation<InventorySyncStatus>()
                .Add(x => x.IsMatched, "Stock Item is not matched with a Shopify Variant")
                .Add(x => !x.IsShopifyVariantMissing, $"Matched Shopify Variant is missing")
                .Add(x => !x.IsSkuMismatchedWithItemId, $"Shopify Variant SKU is mismatched with Acumatica Stock Item ID");

            return validation.Run(this);
        }

        public ValidationResult ReadyToSyncInventory()
        {
            var validation = new Validation<InventorySyncStatus>()
                .Add(x => x.IsMatched, "Stock Item is not matched with a Shopify Variant")
                .Add(x => !x.IsShopifyVariantMissing, $"Matched Shopify Variant is missing")
                .Add(x => !x.IsSkuMismatchedWithItemId, $"Shopify Variant SKU is mismatched with Acumatica Stock Item ID");

            return validation.Run(this);
        }
    }
}

