using System.Linq;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class OrderExtensions
    {

        public static AcumaticaSalesOrder 
                    MatchingSalesOrder(this ShopifyOrder order)
        {
            if (!order.ShopAcuOrderSyncs.Any())
            {
                return null;
            }
            else
            {
                return order
                    .ShopAcuOrderSyncs
                    .First()
                    .AcumaticaSalesOrder;
            }
        }


        public static bool HasMatch(this ShopifyOrder order)
        {
            return order.MatchingSalesOrder() != null;
        }

        public static string AcumaticaSalesOrderId(this ShopifyOrder order)
        {
            return order.HasMatch() ? order.MatchingSalesOrder().AcumaticaOrderNbr : null;
        }


        public static bool
                IsFromShopify(this AcumaticaSalesOrder order)
        {
            // TODO - add intelligence to check for our Monster Stamp
            return order.ShopAcuOrderSyncs.Any();
        }

        public static ShopifyOrder
                MatchingShopifyOrder(this AcumaticaSalesOrder order)
        {
            return order.ShopAcuOrderSyncs.FirstOrDefault()?.ShopifyOrder;
        }

    }
}
