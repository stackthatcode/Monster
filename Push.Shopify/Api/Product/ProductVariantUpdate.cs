using System.Collections.Generic;

namespace Push.Shopify.Api.Product
{
    public class ProductVariantUpdate
    {
        public long id { get; set; }
        public List<Variant> variants { get; set; }
    }

    public class ProductVariantUpdateParent
    {
        public ProductVariantUpdate product { get; set; }

        public ProductVariantUpdateParent(ProductVariantUpdate product)
        {
            this.product = product;
        }
    }
}
