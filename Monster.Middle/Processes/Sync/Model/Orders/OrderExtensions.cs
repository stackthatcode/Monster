﻿using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Misc;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class OrderExtensions
    {
        public static bool HasErrorsPastThreshold(this ShopifyOrder order)
        {
            return order.PutErrorCount >= SystemConsts.ErrorThreshold ||
                   (order.AcumaticaSalesOrder != null && order.AcumaticaSalesOrder
                        .AcumaticaSoShipments.Any(x => x.PutErrorCount >= SystemConsts.ErrorThreshold));
        }


        public static AcumaticaSalesOrder SyncedSalesOrder(this ShopifyOrder order)
        {
            return order.AcumaticaSalesOrder;
        }

        public static bool ExistsInAcumatica(this ShopifyOrder order)
        {
            return order.SyncedSalesOrder() != null; // && !order.IsEmptyOrCancelled;
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

    }
}
