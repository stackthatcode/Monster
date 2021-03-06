﻿using System.Linq;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Analysis;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class OrderExtensions
    {

        public static bool IsEmptyOrCancelled(this ShopifyOrder order)
        {
            return order.ShopifyAreAllItemsRefunded || order.ShopifyIsCancelled;
        }

        public static AcumaticaSalesOrder SyncedSalesOrder(this ShopifyOrder order)
        {
            return order.AcumaticaSalesOrder;
        }

        public static bool HasSyncWithUnknownNbr(this ShopifyOrder order)
        {
            return order.AcumaticaSalesOrder != null &&
                   (order.AcumaticaSalesOrder.AcumaticaOrderNbr == null ||
                   order.AcumaticaSalesOrder.AcumaticaOrderNbr == AcumaticaSyncConstants.UnknownRefNbr);
        }


        public static bool ExistsInAcumatica(this ShopifyOrder order)
        {
            return order.SyncedSalesOrder() != null; // && !order.IsEmptyOrCancelled;
        }

        public static bool HasFulfillmentsNotFromAcumatica(this ShopifyOrder order)
        {
            return order.ShopifyFulfillments.Any(
                x => x.AcumaticaSoShipments == null || !x.AcumaticaSoShipments.Any());
        }

        public static bool HasUnsyncedFulfillments(this ShopifyOrder order)
        {
            return order.SoShipments().Any(x => x.ShopifyFulfillment == null);
        }

        public static bool HasInvoicedShipments(this ShopifyOrder order)
        {
            return order.SoShipments().Any();
        }


        public static string AcumaticaSalesOrderId(this ShopifyOrder order)
        {
            return order.ExistsInAcumatica() ? order.SyncedSalesOrder().AcumaticaOrderNbr : null;
        }
        public static string AcumaticaCustomerId(this ShopifyOrder order)
        {
            return order.ExistsInAcumatica() ? order.SyncedSalesOrder().AcumaticaCustomer.AcumaticaCustomerId : null;
        }

        public static ShopifyOrder OriginalShopifyOrder(this AcumaticaSalesOrder order)
        {
            return order.ShopifyOrder;
        }

        public static decimal AcumaticaCreditMemosTotal(this ShopifyOrder order)
        {
            return order.ShopifyRefunds
                .Where(x => x.CreditAdjustment > 0 && x.AcumaticaMemo != null)
                .Sum(x => x.AcumaticaMemo.AcumaticaAmount);
        }

        public static decimal AcumaticaDebitMemosTotal(this ShopifyOrder order)
        {
            return order.ShopifyRefunds
                .Where(x => x.DebitAdjustment > 0 && x.AcumaticaMemo != null)
                .Sum(x => x.AcumaticaMemo.AcumaticaAmount);
        }

        public static bool ExceedsErrorLimit(this ShopifyOrder order)
        {
            return order.ErrorCount >= SystemConsts.ErrorThreshold;
        }

        public static bool DoesNotExceedErrorLimit(this ShopifyOrder order)
        {
            return !order.ExceedsErrorLimit();
        }
    }
}
