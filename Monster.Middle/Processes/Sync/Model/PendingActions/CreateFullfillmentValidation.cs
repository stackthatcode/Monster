using System.Collections.Generic;
using Push.Foundation.Utilities.Validation;

namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public class CreateFulfillmentValidation
    {
        public bool AnyShopifyMadeFulfillments { get; set; }
        public bool WarehouseLocationUnmatched { get; set; }
        public List<string> UnmatchedVariantStockItems { get; set; }
        public bool ErrorThresholdExceeded { get; set; }

        public ValidationResult Result()
        {
            var validation = new Validation<CreateFulfillmentValidation>()
                .Add(x => !x.AnyShopifyMadeFulfillments,
                    "Corrupted state - fulfillments were created in Shopify by other means")
                .Add(x => !x.WarehouseLocationUnmatched, "Unmatched Warehouse Locations")
                .Add(x => x.UnmatchedVariantStockItems.Count == 0, "Unmatched Variants and Stock Items")
                .Add(x => !x.ErrorThresholdExceeded, "Encountered too many errors attempting to synchronize Acumatica Shipment");
            return validation.Run(this);
        }

        public CreateFulfillmentValidation()
        {
            AnyShopifyMadeFulfillments = false;
            WarehouseLocationUnmatched = false;
            UnmatchedVariantStockItems = new List<string>();
        }
    }
}

