using System.Linq;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class OrderExtensions
    {
        public static AcumaticaSalesOrder SyncedSalesOrder(this ShopifyOrder order)
        {
            return order.AcumaticaSalesOrder;
        }

        public static bool IsSynced(this ShopifyOrder order)
        {
            return order.SyncedSalesOrder() != null;
        }

        public static string AcumaticaSalesOrderId(this ShopifyOrder order)
        {
            return order.IsSynced() ? order.SyncedSalesOrder().AcumaticaOrderNbr : null;
        }

        public static ShopifyOrder OriginalShopifyOrder(this AcumaticaSalesOrder order)
        {
            return order.ShopifyOrder;
        }

    }
}
