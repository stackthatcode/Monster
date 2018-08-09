namespace Push.Shopify.Api.Order
{
    public class ValueType
    {
        public const string FixedAmount = "fixed_amount";
        public const string Percentage = "percentage";
    }

    public class DiscountTargetType
    {
        public const string LineItem = "line_item";
        public const string ShippingLine = "shipping_line";
    }

    public class DiscountTargetSelection
    {
        public const string Manual = "manual";
        public const string Percentage = "percentage";
    }

    public class DiscountApplicationType
    {
        public const string Manual = "manual";
        public const string Percentage = "percentage";
        public const string DiscountCode = "discount_code";
    }

    public class DiscountAllocationMethod
    {
        public const string Across = "across";
        public const string Each = "each";
        public const string One = "one";
    }
}


