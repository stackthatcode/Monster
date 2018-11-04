namespace Monster.Middle.Persist.Multitenant.Extensions
{
    public static class OrderExtensions
    {
        public static bool LineItemsAreReadyToSync(this UsrShopifyOrder order)
        {
            foreach (var item in order.UsrShopifyOrderLineItems)
            {
                if (item.UsrShopifyVariant == null)
                {
                    return false;
                }

                if (item.UsrShopifyVariant.IsMissing)
                {
                    return false;
                }

                if (item.UsrShopifyVariant.IsNotMatched())
                {
                    return false;
                }
            }

            return true;
        }
    }
}

