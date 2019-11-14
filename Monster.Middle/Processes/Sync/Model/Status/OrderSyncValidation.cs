using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<long> LineItemIdsWithUnrecognizedVariants { get; set; }
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
            var validation = new Validation<OrderSyncValidation>()

                .Add(x => !x.IsFulfilledBeforeSync,
                    $"Shopify Order has been fulfilled before sync with Acumatica", instantFailure: true)

                .Add(x => !x.IsCancelledBeforeSync, $"Shopify Order has been cancelled before sync with Acumatica")

                .Add(x => x.HasShopifyCustomer, "Shopify Customer has not been downloaded yet")

                .Add(x => x.ShopifyOrderRecord.HasPayment(), "Shopify Payment has not been downloaded yet")

                .Add(x => HasValidGateway, $"Does not have a valid payment gateway; please check configuration")

                .Add(x => x.LineItemIdsWithUnrecognizedVariants.Count == 0, 
                        "Shopify Order references manually created Variants")

                .Add(x => x.SkusNotSyncedInAcumatica.Count == 0,
                        x => $"References Variants not synced with Acumatica: " +
                            x.SkusNotSyncedInAcumatica.StringJoin(","))

                .Add(x => x.SkusWithMismatchedStockItemId.Count == 0,
                    x => $"Shopify Variants SKU's are mismatched with Acumatica Stock Items ID's: " +
                            x.SkusWithMismatchedStockItemId.StringJoin(","))

                .Add(x => x.SkusWithMismatchedTaxes.Count == 0,
                        x => $"Has Shopify Variants that mismatch with Acumatica Tax Category: " +
                            x.SkusWithMismatchedTaxes.StringJoin(","))

                .Add(x => x.OrderNumberValidForSync,
                    $"Shopify Order number not greater than or equal to Settings -> Starting Order Number");

            return validation.Run(this);
        }

        public OrderSyncValidation()
        {
            LineItemIdsWithUnrecognizedVariants = new List<long>();
            SkusNotSyncedInAcumatica = new List<string>();
            SkusWithMismatchedStockItemId = new List<string>();
            SkusWithMismatchedTaxes = new List<string>();
        }
    }
}

