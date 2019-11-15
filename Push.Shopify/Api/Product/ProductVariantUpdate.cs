using System.Collections.Generic;

namespace Push.Shopify.Api.Product
{
    public class ProductVariantUpdate
    {
        public long id { get; set; }
        public List<VariantNew> variants { get; set; }
    }
}
