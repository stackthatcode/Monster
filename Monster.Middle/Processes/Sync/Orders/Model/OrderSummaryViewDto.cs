namespace Monster.Middle.Processes.Sync.Orders.Model
{
    public class OrderSummaryViewDto
    {
        public long ShopifyOrderId { get; set; }
        public int ShopifyOrderNumber { get; set; }
        public string AcumaticaOrderNbr { get; set; }
        public string AcumaticaShipmentNbr { get; set; }
        public string AcumaticaInvoiceNbr { get; set; }


        public string ShopifyOrderUrl { get; set; }
        public string AcumaticaOrderUrl { get; set; }
        public string AcumaticaShipmentUrl { get; set; }
        public string AcumaticaShipmentInvoiceUrl { get; set; }

    }
}
