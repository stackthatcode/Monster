using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Persist.Matching;
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
        public bool AreIdentifiersMismatched
            => IsMatched && Canonizers.Sku(StockItemId) != Canonizers.Sku(ShopifyVariantSku);
        

        public static InventorySyncStatus Make(AcumaticaStockItem input)
        {
            var output = new InventorySyncStatus();
            output.StockItemId = input.ItemId;

            output.IsStockItemPriceSynced = input.IsPriceSynced;
            output.IsStockItemInventorySynced = input.AcumaticaWarehouseDetails.All(x => x.IsInventorySynced);

            if (input.HasMatch())
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
                .Add(x => !x.IsStockItemPriceSynced, "Stock Item is not flagged for needing Price update")
                .Add(x => !x.IsShopifyVariantMissing, $"Matched Shopify Variant is missing");

            return validation.Run(this);
        }

        public ValidationResult ReadyToSyncInventory()
        {
            var validation = new Validation<InventorySyncStatus>()
                .Add(x => x.IsMatched, "Stock Item is not matched with a Shopify Variant")
                .Add(x => !x.IsStockItemInventorySynced, "Stock Item is not flagged for needing Inventory update")
                .Add(x => !x.IsShopifyVariantMissing, $"Matched Shopify Variant is missing");

            return validation.Run(this);
        }
    }
}

