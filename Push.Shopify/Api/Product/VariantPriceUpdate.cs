namespace Push.Shopify.Api.Product
{
    public class VariantPriceUpdate
    {
        public long id { get; set; }
        public decimal price { get; set; }
        public bool taxable { get; set; }
        public decimal? compare_at_price { get; set; }
    }

    public class VariantPriceUpdateParent
    {
        public VariantPriceUpdate variant { get; set; }

        public static VariantPriceUpdateParent Make(long id, decimal price, bool taxable)
        {
            var dto = new VariantPriceUpdate
            {
                id = id,
                price = price,
                taxable = taxable
            };

            return new VariantPriceUpdateParent()
            {
                variant = dto,
            };
        }
    }
}

