namespace Monster.Web.Models.Analysis
{
    public class OrderAnalyzerGridRow
    {
        public string ShopifyOrderNbr { get; set; }
        public string ShopifyOrderHref { get; set; }
        public string AcumaticaSalesOrderNbr { get; set; }
        public string AcumaticaSalesOrderHref { get; set; }
        public string ShopifyOrderTotal { get; set; }
        public string ShopifyNetPayment { get; set; }
        public string AcumaticaOrderPayment { get; set; }
        public string AcumaticaNetPayment { get; set; }
        public string AcumaticaInvoiceTotal { get; set; }
    }
}
