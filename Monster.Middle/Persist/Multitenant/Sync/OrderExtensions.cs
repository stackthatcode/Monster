using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Sync
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
