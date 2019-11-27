namespace Monster.Middle.Processes.Sync.Model.Analysis
{
    public class OrderAnalysisTotals
    {
        public long ShopifyOrderId { get; set; }
        public string ShopifyOrderNbr { get; set; }
        public string ShopifyOrderHref { get; set; }

        public decimal ShopifyTotalLinePrice { get; set; }
        public decimal ShopifyShippingPriceTotal { get; set; }
        public decimal ShopifyTotalTax { get; set; }
        public decimal ShopifyOrderTotal { get; set; }

        public decimal ShopifyOrderPayment { get; set; }
        public decimal ShopifyRefundPayment { get; set; }
        public decimal ShopifyNetPayment { get; set; }

        public decimal ShopifyRefundItemTotal { get; set; }
        public decimal ShopifyRefundShippingTotal { get; set; }
        public decimal ShopifyRefundTaxTotal { get; set; }
        public decimal ShopifyCreditTotal { get; set; }
        public decimal ShopifyDebitTotal { get; set; }
        public decimal ShopifyRefundTotal { get; set; }
        public decimal ShopifyRefundOverpayment { get; set; }

        public string AcumaticaSalesOrderNbr { get; set; }
        public string AcumaticaSalesOrderHref { get; set; }
        public decimal AcumaticaOrderLineTotal { get; set; }
        public decimal AcumaticaOrderFreight { get; set; }
        public decimal AcumaticaTaxTotal { get; set; }
        public decimal AcumaticaOrderTotal { get; set; }
        public decimal AcumaticaPaymentTotal { get; set; }
        public decimal AcumaticaRefundPaymentTotal { get; set; }
        public decimal AcumaticaNetPaymentTotal { get; set; }
        public decimal AcumaticaRefundCreditTotal { get; set; }
        public decimal AcumaticaRefundDebitTotal { get; set; }
        public decimal AcumaticaCreditDebitMemoTotal { get; set; }
        public decimal AcumaticaInvoicePriceTotal { get; set; }
        public decimal AcumaticaInvoiceFreightTotal { get; set; }
        public decimal AcumaticaInvoiceTaxTotal { get; set; }
        public decimal AcumaticaInvoiceTotal { get; set; }


        public OrderAnalysisTotals()
        {
            ShopifyOrderNbr = AnalysisExtensions.MissingField;
            ShopifyOrderHref = null;

            ShopifyTotalLinePrice = 0m;
            ShopifyShippingPriceTotal = 0m;
            ShopifyTotalTax = 0m;
            ShopifyOrderTotal = 0m;

            ShopifyOrderPayment = 0m;
            ShopifyRefundPayment = 0m;
            ShopifyNetPayment = 0m;

            ShopifyRefundItemTotal = 0m;
            ShopifyRefundShippingTotal = 0m;
            ShopifyRefundTaxTotal = 0m;
            ShopifyCreditTotal = 0m;
            ShopifyDebitTotal = 0m;
            ShopifyRefundTotal = 0m;

            AcumaticaSalesOrderNbr = AnalysisExtensions.MissingField;
            AcumaticaSalesOrderHref = null;
            AcumaticaOrderLineTotal = 0m;
            AcumaticaOrderFreight = 0m;
            AcumaticaTaxTotal = 0m;
            AcumaticaOrderTotal = 0m;

            AcumaticaPaymentTotal = 0m;
            AcumaticaRefundPaymentTotal = 0m;
            AcumaticaNetPaymentTotal = 0m;
            AcumaticaRefundCreditTotal = 0m;
            AcumaticaRefundDebitTotal = 0m;
            AcumaticaCreditDebitMemoTotal = 0m;

            AcumaticaInvoicePriceTotal = 0m;
            AcumaticaInvoiceTaxTotal = 0m;
            AcumaticaInvoiceFreightTotal = 0m;
            AcumaticaInvoiceTotal = 0m;
        }
    }
}
