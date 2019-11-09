using System.Collections.Generic;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Validation;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class OrderSyncValidation
    {
        public long SettingsStartingOrderId { get; set; }
        public ShopifyOrder ShopifyOrderRecord { get; set; }
        public Order ShopifyOrder { get; set; }
        public List<LineItem> LineItemsWithUnsyncedVariants { get; set; }
        public ShopifyPaymentGateway PaymentGateway { get; set;}


        // Computed
        //
        public bool HasShopifyCustomer => ShopifyOrderRecord.ShopifyCustomer != null;
        public bool HasValidGateway => PaymentGateway != null;
        public bool HasManualProductVariants => ShopifyOrder.LineItemsWithManualVariants.Count > 0;
        public bool HasUnmatchedVariants => LineItemsWithUnsyncedVariants.Count > 0;
        public bool OrderNumberValidForSync => ShopifyOrder.id >= SettingsStartingOrderId;
        public bool HasBeenSynced => ShopifyOrderRecord.AcumaticaSalesOrder != null;
        public bool IsCancelledBeforeSync => !HasBeenSynced && ShopifyOrder.cancelled_at != null;
        public bool IsFulfilledBeforeSync 
                => !HasBeenSynced && 
                   ShopifyOrder.fulfillment_status.HasValue() &&
                   ShopifyOrder.fulfillment_status != FulfillmentStatus.NoFulfillment;

        
        public ValidationResult IsReadyToSync()
        {
            var validation = new Validation<OrderSyncValidation>()

                .Add(x => !x.IsFulfilledBeforeSync, 
                    $"Shopify Order has been fulfilled before sync with Acumatica", instantFailure:true)

                .Add(x => !x.IsCancelledBeforeSync, $"Shopify Order has been cancelled before sync with Acumatica")

                .Add(x => x.HasShopifyCustomer, "Shopify Customer has not been downloaded yet")

                .Add(x => x.ShopifyOrderRecord.HasPayment(), "Shopify Payment has not been downloaded yet")

                .Add(x => HasValidGateway, "Does not have valid/supported Payment Gateway")

                .Add(x => !x.HasManualProductVariants, "Shopify Order references manually created Variants")

                .Add(x => !x.HasUnmatchedVariants, "Shopify Order references Variants not synced with Acumatica")

                .Add(x => x.OrderNumberValidForSync,
                    $"Shopify Order number not greater than or equal to Settings -> Starting Order Number");

            return validation.Run(this);
        }        
    }
}

