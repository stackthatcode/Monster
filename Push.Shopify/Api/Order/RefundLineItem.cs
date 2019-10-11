namespace Push.Shopify.Api.Order
{
    public class RefundLineItem
    {
        public long id { get; set; }
        public int quantity { get; set; }
        public long line_item_id { get; set; }
        public long? location_id { get; set; }
        public string restock_type { get; set; }
        public decimal subtotal { get; set; }
        public decimal total_tax { get; set; }

        public bool IsTaxable => total_tax > 0;
    }
}

