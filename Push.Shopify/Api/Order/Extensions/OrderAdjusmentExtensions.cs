namespace Push.Shopify.Api.Order.Extensions
{
    public static class OrderAdjusmentExtensions
    {
        public static bool IsShippingAdjustment(this OrderAdjustment adjustment)
        {
            return adjustment.kind == "shipping_refund";
        }
    }
}

