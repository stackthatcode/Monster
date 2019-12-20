using Push.Foundation.Utilities.Json;

namespace Push.Shopify.Api.Product
{
    public class VariantUpdate
    {
        public long id { get; set; }
        public bool taxable { get; set; }
        public decimal? price { get; set; }
        public int? grams { get; set; }

        public string ToJson()
        {
            return new
            {
                variant = this
            }.SerializeToJson();
        }
    }
}

