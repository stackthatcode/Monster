using System.Collections.Generic;

namespace Push.Shopify.Api.Product
{
    public class ProductNew
    {
        public string title { get; set; }
        public string vendor { get; set; }
        public string product_type { get; set; }
        public List<ShopifyVariantNew> variants { get; set; }
    }
}
