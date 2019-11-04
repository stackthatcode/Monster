using System.Linq;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class OrderExtensions
    {
        public static AcumaticaSalesOrder MatchingSalesOrder(this ShopifyOrder order)
        {
            return order.AcumaticaSalesOrder;
        }

        public static bool HasMatch(this ShopifyOrder order)
        {
            return order.MatchingSalesOrder() != null;
        }

        public static string AcumaticaSalesOrderId(this ShopifyOrder order)
        {
            return order.HasMatch() ? order.MatchingSalesOrder().AcumaticaOrderNbr : null;
        }

        public static ShopifyOrder MatchingShopifyOrder(this AcumaticaSalesOrder order)
        {
            return order.ShopifyOrder;
        }

    }
}
