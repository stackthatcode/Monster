using System.Linq;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Transactions;


namespace Monster.Middle.Processes.Shopify.Persist
{
    public static class OrderExtensions
    {
        public static Order ToShopifyObj(this ShopifyOrder input)
        {
            return input.ShopifyJson.DeserializeToOrder();
        }

        public static Transaction ToShopifyObj(this ShopifyTransaction input)
        {
            return input.ShopifyJson.DeserializeFromJson<Transaction>();
        }

        public static Refund ToRefundObj(this ShopifyRefund input)
        {
            var shopifyOrderRecord = input.ShopifyOrder;
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();
            return shopifyOrder.refunds.First(x => x.id == input.ShopifyRefundId);
        }
    }
}

