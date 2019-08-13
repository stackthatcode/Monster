using System.Linq;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Extensions
{
    public static class OrderExtensions
    {

        public static UsrAcumaticaSalesOrder 
                    MatchingSalesOrder(this UsrShopifyOrder order)
        {
            if (!order.UsrShopAcuOrderSyncs.Any())
            {
                return null;
            }
            else
            {
                return order
                    .UsrShopAcuOrderSyncs
                    .First()
                    .UsrAcumaticaSalesOrder;
            }
        }


        public static bool HasMatch(this UsrShopifyOrder order)
        {
            return order.MatchingSalesOrder() != null;
        }

        public static string AcumaticaSalesOrderId(this UsrShopifyOrder order)
        {
            return order.HasMatch() ? order.MatchingSalesOrder().AcumaticaOrderNbr : null;
        }


        public static bool
                IsFromShopify(this UsrAcumaticaSalesOrder order)
        {
            // TODO - add intelligence to check for our Monster Stamp
            return order.UsrShopAcuOrderSyncs.Any();
        }

        public static UsrShopifyOrder
                MatchingShopifyOrder(this UsrAcumaticaSalesOrder order)
        {
            return order.UsrShopAcuOrderSyncs.FirstOrDefault()?.UsrShopifyOrder;
        }

    }
}
