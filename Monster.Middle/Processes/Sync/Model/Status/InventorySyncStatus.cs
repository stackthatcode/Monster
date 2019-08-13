using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Extensions;
using Monster.Middle.Processes.Sync.Model.Misc;
using Push.Foundation.Utilities.Validation;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class InventorySyncStatus
    {
        public string StockItemId { get; set; }
        public bool StockItemIsInventorySynced { get; set; }
        public long? ShopifyVariantId { get; set; }
        public string ShopifyVariantSku { get; set; }
        public bool IsShopifyVariantMissing { get; set; }

        // Computed
        public bool IsMatched => ShopifyVariantId.HasValue;
        public bool AreIdentifiersMismatched
            => IsMatched && Standards.Sku(StockItemId) != Standards.Sku(ShopifyVariantSku);
        

        public static InventorySyncStatus Make(UsrAcumaticaStockItem input)
        {
            var output = new InventorySyncStatus();
            output.StockItemId = input.ItemId;

            if (input.HasMatch())
            {
                output.ShopifyVariantId = input.MatchedVariant().ShopifyVariantId;
                output.ShopifyVariantSku = input.MatchedVariant().ShopifySku;
                output.IsShopifyVariantMissing = input.MatchedVariant().IsMissing;
            }

            return output;
        }
        
        public ValidationResult ReadyToSync()
        {
            var validation = new Validation<InventorySyncStatus>()
                .Add(x => x.IsMatched, "Stock Item is not matched with a Shopify Variant")
                .Add(x => !x.StockItemIsInventorySynced, "Stock Item is not flagged for needing update")
                .Add(x => !x.IsShopifyVariantMissing, $"Matched Shopify Variant is missing");

            return validation.Run(this);
        }
    }
}
