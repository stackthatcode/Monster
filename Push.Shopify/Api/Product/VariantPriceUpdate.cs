namespace Push.Shopify.Api.Product
{
    public class VariantPriceUpdate
    {
        public long id { get; set; }
        public double price { get; set; }
        public double? compare_at_price { get; set; }
    }

    public class VariantPriceUpdateParent
    {
        public VariantPriceUpdate variant { get; set; }

        public static VariantPriceUpdateParent
                Make(long id, double price, double? compare_at_price = null)
        {
            var dto = new VariantPriceUpdate
            {
                id = id,
                price = price
            };
            if (compare_at_price.HasValue)
            {
                dto.compare_at_price = compare_at_price;
            }

            return new VariantPriceUpdateParent()
            {
                variant = dto,
            };
        }
    }
}

