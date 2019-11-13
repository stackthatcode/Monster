﻿using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;

namespace Monster.Middle.Processes.Sync.Model.Analysis
{
    public static class AnalysisExtensions
    {
        public const string MissingField = "N/A";

        public static string AnalysisFormat(this decimal? input)
        {
            return (input ?? 0).ToString("0.00");
        }

        public static string AnalysisFormat(this decimal input)
        {
            return AnalysisFormat((decimal?)input);
        }

        public static string AnalysisFormat(this double input)
        {
            return AnalysisFormat((decimal)input);
        }


        public static decimal ShopifyPaymentAmount(this ShopifyOrder order)
        {
            return order.PaymentTransaction() != null
                ? order.PaymentTransaction().ShopifyAmount
                : 0m;
        }


        public static decimal ShopifyNetPayment(this ShopifyOrder order)
        {
            return order.ShopifyPaymentAmount()
                   - order.RefundTransactions().Sum(x => x.ShopifyAmount);
        }

        public static bool IsPaymentSynced(this ShopifyOrder order)
        {
            return order.PaymentTransaction().AcumaticaPayment != null;
        }

        public static decimal AcumaticaPaymentAmount(this ShopifyOrder order)
        {
            return order.IsPaymentSynced() ?
                order.PaymentTransaction().AcumaticaPayment.AcumaticaAmount : 0m;
        }

        public static decimal AcumaticaCustomerRefundTotal(this ShopifyOrder order)
        {
            return order.RefundTransactions().Sum(x => x.AcumaticaPayment?.AcumaticaAmount ?? 0m);
        }
        public static decimal AcumaticaNetPaymentAmount(this ShopifyOrder order)
        {
            return order.AcumaticaPaymentAmount() - order.AcumaticaCustomerRefundTotal();
        }

        public static decimal AcumaticaInvoiceTaxTotal(this ShopifyOrder order)
        {
            return order.AcumaticaSalesOrder.AcumaticaSoShipments.Sum(x => x.AcumaticaInvoiceTax ?? 0m);
        }

        public static decimal AcumaticaInvoiceTotal(this ShopifyOrder order)
        {
            return order.AcumaticaSalesOrder.AcumaticaSoShipments.Sum(x => x.AcumaticaInvoiceAmount ?? 0m);
        }
    }
}