using System.Linq;
using AutoMapper;
using Monster.Middle.Persist.Instance;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class OrderExtensions
    {
        public static AcumaticaSalesOrder MatchingSalesOrder(this ShopifyOrder order)
        {
            if (!order.ShopAcuOrderSyncs.Any())
            {
                return null;
            }
            else
            {
                return order.ShopAcuOrderSyncs.First().AcumaticaSalesOrder;
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

        public static bool IsFromShopify(this AcumaticaSalesOrder order)
        {
            return order.ShopAcuOrderSyncs.Any();
        }

        public static ShopifyOrder MatchingShopifyOrder(this AcumaticaSalesOrder order)
        {
            return order.ShopAcuOrderSyncs.FirstOrDefault()?.ShopifyOrder;
        }

        public static bool HasMatch(this AcumaticaSalesOrder order)
        {
            return order.ShopAcuOrderSyncs.Any();
        }

        public static ShopAcuOrderSync Sync(this AcumaticaSalesOrder order)
        {
            return order.ShopAcuOrderSyncs.FirstOrDefault();
        }

        public static ShopAcuOrderSync Sync(this ShopifyOrder order)
        {
            return order.ShopAcuOrderSyncs.FirstOrDefault();
        }

    }
}
