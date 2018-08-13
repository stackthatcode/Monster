namespace Push.Shopify.Api.Order
{
    public class RefundTaxLine
    {        
        public decimal Price { get; set; }
        public decimal Rate { get; set; }
        public string Title { get; set; }
    }
}
