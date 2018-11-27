using Monster.Middle.Persist.Multitenant;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public static class OrderExtensions
    {
        public static Order ToShopifyObj(this UsrShopifyOrder input)
        {
            return input.ShopifyJson.DeserializeToOrder();
        }
    }
}
