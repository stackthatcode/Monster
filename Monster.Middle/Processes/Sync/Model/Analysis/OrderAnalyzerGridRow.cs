namespace Monster.Middle.Processes.Sync.Model.Analysis
{
    public class OrderAnalyzerGridRow
    {
        public string ShopifyOrderNbr { get; set; }
        public string ShopifyOrderHref { get; set; }
        public string ShopifyOrderTotal { get; set; }
        public string ShopifyNetPayment { get; set; }

        public string AcumaticaSalesOrderNbr { get; set; }
        public string AcumaticaSalesOrderHref { get; set; }
        public string AcumaticaOrderPayment { get; set; }
        public string AcumaticaNetPayment { get; set; }
        public string AcumaticaInvoiceTotal { get; set; }

        public string OutstandingBalance { get; set; }

        public OrderAnalyzerGridRow()
        {
            AcumaticaSalesOrderNbr = AnalysisExtensions.MissingField;
            AcumaticaOrderPayment = AnalysisExtensions.MissingField;
            AcumaticaNetPayment = AnalysisExtensions.MissingField;
            AcumaticaInvoiceTotal = AnalysisExtensions.MissingField;

            OutstandingBalance = "0.00";
        }
    }
}
