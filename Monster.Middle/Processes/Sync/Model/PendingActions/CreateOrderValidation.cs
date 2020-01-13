using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Validation;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public class CreateOrderValidation
    {
        public long SettingsStartingOrderId { get; set; }
        public ShopifyOrder ShopifyOrderRecord { get; set; }
        public Order ShopifyOrder { get; set; }

        public List<long> LineItemIdsWithUnrecognizedSku { get; set; }
        public List<string> SkusMissingFromShopify { get; set; }
        public List<string> SkusNotSyncedInAcumatica { get; set; }
        public List<string> SkusWithMismatchedStockItemId { get; set; }
        public List<string> SkusWithMismatchedTaxes { get; set; }

        public string ShopifyPaymentGatewayId { get; set; }
        public bool HasValidGateway { get; set; }


        // Computed for validation
        //
        public bool HasShopifyCustomer => ShopifyOrderRecord.ShopifyCustomer != null;
        public bool OrderNumberValidForSync => ShopifyOrder.id >= SettingsStartingOrderId;
        public bool HasBeenSynced => ShopifyOrderRecord.AcumaticaSalesOrder != null;
        public bool IsCancelledBeforeSync => !HasBeenSynced && ShopifyOrder.cancelled_at != null;

        public bool IsFulfilledBeforeSync
            => !HasBeenSynced &&
               ShopifyOrder.fulfillment_status.HasValue() &&
               ShopifyOrder.fulfillment_status != FulfillmentStatus.NoFulfillment;


        public ValidationResult Result()
        {
            var validation = new Validation<CreateOrderValidation>()
                .Add(x => x.ShopifyOrderRecord.PutErrorCount < SystemConsts.ErrorThreshold,
                        "Encountered too many errors attempting to synchronize this Shopify Order")

                .Add(x => !x.IsFulfilledBeforeSync,
                        $"Shopify Order has been fulfilled before sync with Acumatica", instantFailure: true)
                
                .Add(x => !x.ShopifyOrder.AllLineItemsRefunded, "All Line Items have been refunded")

                //.Add(x => !x.IsCancelledBeforeSync, $"Shopify Order has been cancelled before sync with Acumatica")
                
                .Add(x => x.HasShopifyCustomer, "Shopify Customer has not been downloaded yet")
                
                .Add(x => x.ShopifyOrderRecord.HasPayment(), "Shopify Payment has not been downloaded yet")
                
                .Add(x => HasValidGateway, $"Does not have a valid payment gateway; please check configuration")
                
                .Add(x => x.OrderNumberValidForSync,
                        $"Shopify Order number not greater than or equal to Settings -> Starting Order Number")

                // The cardinal Synchronization Sins
                //
                .Add(x => !x.LineItemIdsWithUnrecognizedSku.Any(), "Shopify Order contains line item(s) without a SKU")
                
                .Add(x => !x.SkusMissingFromShopify.Any(), 
                        x => $"Shopify Order contains line item(s) that reference missing Variant(s) {x.SkusMissingFromShopify.StringJoin(", ")}")
                
                .Add(x => !x.SkusNotSyncedInAcumatica.Any(),
                        x => $"Shopify Order contains Variant(s) not synced with Acumatica: {x.SkusNotSyncedInAcumatica.StringJoin(", ")}")
                
                .Add(x => !x.SkusWithMismatchedStockItemId.Any(),
                        x => $"Shopify Order contains Variant(s) with SKU's that mismatch with Acumatica Stock Items: " +
                        x.SkusWithMismatchedStockItemId.StringJoin(", "))
                
                .Add(x => !x.SkusWithMismatchedTaxes.Any(),
                        x => $"Shopify Order contains Variant(s) that mismatched Taxes with Acumatica: " +
                        x.SkusWithMismatchedTaxes.StringJoin(", "));

            return validation.Run(this);
        }

        public CreateOrderValidation()
        {
            LineItemIdsWithUnrecognizedSku = new List<long>();
            SkusMissingFromShopify = new List<string>();
            SkusNotSyncedInAcumatica = new List<string>();
            SkusWithMismatchedStockItemId = new List<string>();
            SkusWithMismatchedTaxes = new List<string>();
        }
    }
}

