using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Sync
{
    public static class OrderExtensions
    {

        public static
            UsrAcumaticaSalesOrder AcumaticaSalesOrder(this UsrShopifyOrder order)
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

    }
}
