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
                        .AcumaticaSoShipments.Any(x => x.PutErrorCount >= SystemConsts.ErrorThreshold)) ||
                   order.ShopifyTransactions.Any(x => x.PutErrorCount >= SystemConsts.ErrorThreshold);
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

        public static ShopifyOrder OriginalShopifyOrder(this AcumaticaSalesOrder order)
        {
            return order.ShopifyOrder;
        }
    }
}
