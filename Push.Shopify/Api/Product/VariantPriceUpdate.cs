namespace Push.Shopify.Api.Product
{
    public class VariantPriceUpdate
    {
        public long id { get; set; }
        public double price { get; set; }
        public double? compare_at_price { get; set; }
    }
}

