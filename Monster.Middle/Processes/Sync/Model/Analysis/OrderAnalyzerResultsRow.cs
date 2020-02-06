using Monster.Middle.Processes.Sync.Model.PendingActions;

namespace Monster.Middle.Processes.Sync.Model.Analysis
{
    public class OrderAnalyzerResultsRow
    {
        public long ShopifyOrderId { get; set; }

        public string ShopifyOrderNbr { get; set; }
        public string ShopifyOrderHref { get; set; }
        public decimal ShopifyOrderTotal { get; set; }
        public decimal ShopifyNetPayment { get; set; }

        public string ShopifyFinancialStatus { get; set; }
        public string ShopifyFulfillmentStatus { get; set; }
        public bool ShopifyIsCancelled { get; set; }
        public bool ShopifyAreAllItemsRefunded { get; set; }

        public string AcumaticaSalesOrderNbr { get; set; }
        public string AcumaticaSalesOrderHref { get; set; }
        public string AcumaticaStatus { get; set; }
        public decimal AcumaticaOrderPayment { get; set; }
        public decimal AcumaticaNetPayment { get; set; }
        public decimal AcumaticaInvoiceTotal { get; set; }

        public decimal OutstandingBalance { get; set; }
        public bool HasError { get; set; }
        public bool HasPendingActions { get; set; }

        public OrderAnalyzerResultsRow()
        {
            AcumaticaSalesOrderNbr = AnalysisExtensions.MissingField;
            AcumaticaStatus = AnalysisExtensions.MissingField;
            AcumaticaOrderPayment = 0m;
            AcumaticaNetPayment = 0m;
            AcumaticaInvoiceTotal = 0m;

            OutstandingBalance = 0m;
        }
    }
}
