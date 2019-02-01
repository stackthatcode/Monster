namespace Monster.Middle.Processes.Sync.Orders.Model
{
    public class OrderSummaryViewDto
    {
        public long? ShopifyOrderId { get; set; }
        public int? ShopifyOrderNumber { get; set; }
        public string AcumaticaOrderNbr { get; set; }
        public string AcumaticaInvoiceNbr { get; set; }
        public string AcumaticaShipmentNbr { get; set; }

        //public string ShopifyBaseUrl { get; set; }
        //public string AcumaticaBaseUrl { get; set; }

        //public string ShopifyOrderUrl 
        //        => (ShopifyBaseUrl ?? "") + $"/admin/orders/{ShopifyOrderId}";

        //public string AcumaticaOrderUrl
        //        => 
    }
}
