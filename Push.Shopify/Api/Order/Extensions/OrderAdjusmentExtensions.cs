namespace Push.Shopify.Api.Order.Extensions
{
    public static class OrderAdjusmentExtensions
    {
        public static bool IsShippingAdjustment(this OrderAdjustment adjustment)
        {
            return adjustment.kind == "shipping_refund";
        }

        public static bool IsRefundDiscrepancy(this OrderAdjustment adjustment)
        {
            return adjustment.kind == "refund_discrepancy";
        }

    }
}

