﻿using System.Collections.Generic;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Validation;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class OrderSyncStatus
    {
        public long SettingsStartingOrderId { get; set; }
        public ShopifyOrder ShopifyOrderRecord { get; set; }
        public Order ShopifyOrder { get; set; }
        public List<LineItem> LineItemsWithUnsyncedVariants { get; set; }


        // Computed
        //
        public bool HasShopifyCustomer => ShopifyOrderRecord.ShopifyCustomer != null;
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
            var validation = new Validation<OrderSyncStatus>()
                .Add(x => x.HasShopifyCustomer, "Has Shopify Customer")

                .Add(x => !x.HasManualProductVariants, "Shopify Order references Variants with NULL Variant Id")

                .Add(x => !x.HasUnmatchedVariants, "Shopify Order references Variants not synced with Acumatica")

                .Add(x => x.OrderNumberValidForSync, 
                    $"Shopify Order number not greater than or equal to Settings -> Starting Order Number")

                .Add(x => !x.IsCancelledBeforeSync, $"Shopify Order has been canceled before sync with Acumatica")

                .Add(x => !x.IsFulfilledBeforeSync, $"Shopify Order has been fulfilled before sync with Acumatica");

            return validation.Run(this);
        }        
    }
}

