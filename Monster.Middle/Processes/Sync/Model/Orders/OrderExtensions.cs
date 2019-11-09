using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class OrderExtensions
    {
        public static AcumaticaSalesOrder SyncedSalesOrder(this ShopifyOrder order)
        {
            return order.AcumaticaSalesOrder;
        }

        public static bool ExistsInAcumatica(this ShopifyOrder order)
        {
            return order.SyncedSalesOrder() != null;
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
