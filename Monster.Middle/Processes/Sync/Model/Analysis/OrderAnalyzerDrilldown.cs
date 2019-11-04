namespace Monster.Middle.Processes.Sync.Model.Analysis
{
    public class OrderAnalyzerDrilldown
    {
        public string ShopifyOrderNbr { get; set; }
        public string ShopifyOrderHref { get; set; }

        public string ShopifyTotalLinePrice { get; set; }
        public string ShopifyShippingPriceTotal { get; set; }
        public string ShopifyTotalTax { get; set; }
        public string ShopifyOrderTotal { get; set; }

        public string ShopifyOrderPayment { get; set; }
        public string ShopifyRefundPayment { get; set; }
        public string ShopifyNetPayment { get; set; }

        public string ShopifyRefundItemTotal { get; set; }
        public string ShopifyRefundShippingTotal { get; set; }
        public string ShopifyRefundTaxTotal { get; set; }
        public string ShopifyCreditTotal { get; set; }
        public string ShopifyDebitTotal { get; set; }
        public string ShopifyRefundTotal { get; set; }


        public string AcumaticaSalesOrderNbr { get; set; }
        public string AcumaticaOrderLineTotal { get; set; }
        public string AcumaticaOrderFreight { get; set; }
        public string AcumaticaTaxTotal { get; set; }
        public string AcumaticaOrderTotal { get; set; }
        public string AcumaticaPaymentTotals { get; set; }
        public string AcumaticaRefundPaymentTotals { get; set; }
        public string AcumaticaNetPaymentTotal { get; set; }
        public string AcumaticaCreditTotal { get; set; }
        public string AcumaticaRefundDebitTotal { get; set; }
        public string AcumaticaCreditDebitMemoTotal { get; set; }
        public string AcumaticaInvoicePriceTotal { get; set; }
        public string AcumaticaInvoiceTaxTotal { get; set; }
        public string AcumaticaInvoiceFreightTotal { get; set; }
        public string AcumaticaInvoiceTotal { get; set; }

        public OrderAnalyzerDrilldown()
        {
            ShopifyOrderNbr = AnalysisExtensions.MissingField;
            ShopifyOrderHref = AnalysisExtensions.MissingField;

            ShopifyTotalLinePrice = AnalysisExtensions.MissingField;
            ShopifyShippingPriceTotal = AnalysisExtensions.MissingField;
            ShopifyTotalTax = AnalysisExtensions.MissingField;
            ShopifyOrderTotal = AnalysisExtensions.MissingField;

            ShopifyOrderPayment = AnalysisExtensions.MissingField;
            ShopifyRefundPayment = AnalysisExtensions.MissingField;
            ShopifyNetPayment = AnalysisExtensions.MissingField;

            ShopifyRefundItemTotal = AnalysisExtensions.MissingField;
            ShopifyRefundShippingTotal = AnalysisExtensions.MissingField;
            ShopifyRefundTaxTotal = AnalysisExtensions.MissingField;
            ShopifyCreditTotal = AnalysisExtensions.MissingField;
            ShopifyDebitTotal = AnalysisExtensions.MissingField;
            ShopifyRefundTotal = AnalysisExtensions.MissingField;

            AcumaticaSalesOrderNbr = AnalysisExtensions.MissingField;
            AcumaticaOrderLineTotal = AnalysisExtensions.MissingField;
            AcumaticaOrderFreight = AnalysisExtensions.MissingField;
            AcumaticaTaxTotal = AnalysisExtensions.MissingField;
            AcumaticaOrderTotal = AnalysisExtensions.MissingField;

            AcumaticaPaymentTotals = AnalysisExtensions.MissingField;
            AcumaticaRefundPaymentTotals = AnalysisExtensions.MissingField;
            AcumaticaNetPaymentTotal = AnalysisExtensions.MissingField;
            AcumaticaCreditTotal = AnalysisExtensions.MissingField;
            AcumaticaRefundDebitTotal = AnalysisExtensions.MissingField;
            AcumaticaCreditDebitMemoTotal = AnalysisExtensions.MissingField;

            AcumaticaInvoicePriceTotal = AnalysisExtensions.MissingField;
            AcumaticaInvoiceTaxTotal = AnalysisExtensions.MissingField;
            AcumaticaInvoiceFreightTotal = AnalysisExtensions.MissingField;
            AcumaticaInvoiceTotal = AnalysisExtensions.MissingField;
        }
    }
}
