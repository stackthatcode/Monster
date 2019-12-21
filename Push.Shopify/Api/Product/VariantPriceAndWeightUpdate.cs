using Push.Foundation.Utilities.Json;

namespace Push.Shopify.Api.Product
{
    public class VariantUpdate
    {
        public static string Make(long id, bool taxable, decimal? price, int? grams)
        {
            if (price.HasValue && grams.HasValue)
            {
                return new {variant = new {id, taxable, price, grams,}}.SerializeToJson();
            }

            if (price.HasValue && !grams.HasValue)
            {
                return new { variant = new { id, taxable, price,  } }.SerializeToJson();
            }

            if (!price.HasValue && grams.HasValue)
            {
                return new { variant = new { id, taxable, grams, } }.SerializeToJson();
            }

            return new { variant = new { id, taxable } }.SerializeToJson();
        }
    }
}

