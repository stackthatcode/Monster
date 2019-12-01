namespace Push.Shopify.Api.Product
{
    public class ShopifyVariantNew
    {
        public string sku { get; set; }
        public string title { get; set; }
        public string option1 { get; set; }
        public string option2 { get; set; }
        public string option3 { get; set; }
        public bool taxable { get; set; }
        public decimal grams { get; set; }
        public decimal price { get; set; }
        public string inventory_policy { get; set; } 
        public string fulfillment_service { get; set; }
    }
}
