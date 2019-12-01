using System;
using Newtonsoft.Json;

namespace Push.Shopify.Api.Product
{
    public class Variant
    {
        public long id { get; set; }
        public long product_id { get; set; }

        public string sku { get; set; }
        public string title { get; set; }
        public double price { get; set; }
        public double? compare_at_price { get; set; }
        
        public int position { get; set; }
        public string fulfillment_service { get; set; }
        public string option1 { get; set; }
        public string option2 { get; set; }
        public string option3 { get; set; }
        public DateTimeOffset created_at { get; set; }
        public DateTimeOffset updated_at { get; set; }
        public bool taxable { get; set; }
        public string barcode { get; set; }
        public int grams { get; set; }
        public string image_id { get; set; }
        public double weight { get; set; }
        public string weight_unit { get; set; }
        public bool requires_shipping { get; set; }
        
        public string inventory_policy { get; set; }
        public string inventory_management { get; set; }
        public long inventory_item_id { get; set; }


        [JsonIgnore]
        public Product Parent { get; set; }
    }

    public class VariantParent
    {
        public Variant variant { get; set; }
    }
}

