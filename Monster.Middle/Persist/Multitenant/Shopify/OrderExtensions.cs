using Push.Shopify.Api.Order;

namespace Monster.Middle.Persist.Multitenant.Shopify
{
    public static class OrderExtensions
    {
        public static Order ToShopifyObj(this UsrShopifyOrder input)
        {
            return input.ShopifyJson.DeserializeToOrder();
        }
    }
}
