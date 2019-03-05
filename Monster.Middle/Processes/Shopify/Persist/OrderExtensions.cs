using System.Linq;
using Monster.Middle.Persist.Tenant;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public static class OrderExtensions
    {
        public static Order ToShopifyObj(this UsrShopifyOrder input)
        {
            return input.ShopifyJson.DeserializeToOrder();
        }

        public static Refund ToRefundObj(this UsrShopifyRefund input)
        {
            var shopifyOrderRecord = input.UsrShopifyOrder;
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();
            return shopifyOrder.refunds.First(x => x.id == input.ShopifyRefundId);
        }


    }
}
