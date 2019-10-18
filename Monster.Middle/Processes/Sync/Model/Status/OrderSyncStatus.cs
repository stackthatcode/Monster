using System.Collections.Generic;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Validation;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class OrderSyncStatus
    {
        public long ShopifyOrderId { get; set; }
        public long ShopifyOrderNumber { get; set; }        
        public long PreferencesStartingOrderId { get; set; }

        public List<LineItem> LineItemsWithAdhocVariants { get; set; }
        public List<LineItem> LineItemsWithUnmatchedVariants { get; set; }
        public bool IsPaid { get; set; }
        public bool IsCancelled { get; set; }
        public string FulfillmentStatus { get; set; }
        public string AcumaticaSalesOrderId { get; set; }


        // Computed
        //
        public bool UsesAdhocVariants => LineItemsWithAdhocVariants.Count > 0;
        public bool UsesUnmatchedVariants => LineItemsWithUnmatchedVariants.Count > 0;

        public bool OrderNumberValidForSync => ShopifyOrderId >= PreferencesStartingOrderId;
        
        public bool HasBeenSynced => AcumaticaSalesOrderId.HasValue();

        public bool IsCancelledBeforeSync => !HasBeenSynced && IsCancelled;

        public bool IsFulfilledBeforeSync 
                => !HasBeenSynced && FulfillmentStatus.HasValue() &&
                   FulfillmentStatus != Push.Shopify.Api.Order.FulfillmentStatus.NoFulfillment;

        
        public ValidationResult IsReadyToSync()
        {
            var validation = new Validation<OrderSyncStatus>()
                .Add(x => !x.UsesAdhocVariants, 
                    "Shopify Order references Variants with NULL Variant Id")

                .Add(x => !x.UsesUnmatchedVariants, 
                    "Shopify Order references Variants not loaded into Acumatica")

                .Add(x => x.OrderNumberValidForSync, 
                    $"Shopify Order number not greater than or equal to Preferences -> Starting Order Number")

                .Add(x => !x.IsCancelledBeforeSync,
                    $"Shopify Order has been canceled before sync with Acumatica")

                .Add(x => !x.IsFulfilledBeforeSync,
                    $"Shopify Order has been fulfilled before sync with Acumatica");

            return validation.Run(this);
        }        
    }
}

