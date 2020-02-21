using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Model.FinAnalyzer
{
    public static class FinAnalyzerExtensions
    {
        public static FinAnalyzer ToFinAnalyzer(this ShopifyOrder orderRecord, Order shopifyOrder)
        {
            var FinAnalyzer = new FinAnalyzer();
            FinAnalyzer.ExternalRefNbr = shopifyOrder.id.ToString();

            FinAnalyzer.Freight.Price = shopifyOrder.ShippingTotal;
            FinAnalyzer.Freight.TaxableAmount = shopifyOrder.IsShippingTaxable ? shopifyOrder.ShippingTotal : 0m;
            FinAnalyzer.Freight.TaxLines = shopifyOrder.shipping_lines.ToFinAnalyzerTaxLines();
            FinAnalyzer.Freight.TaxAmount = shopifyOrder.ShippingTax;

            foreach (var line_item in shopifyOrder.line_items)
            {
                var xferLineItem = new FinAnalyzerLineItem();
                xferLineItem.ExternalRefNbr = line_item.id.ToString();
                xferLineItem.InventoryID = line_item.sku;
                xferLineItem.Quantity = line_item.quantity;
                xferLineItem.UnitPrice = line_item.UnitPriceAfterDiscount;
                xferLineItem.TaxableAmount = line_item.TaxableAmount;
                xferLineItem.TaxAmount = line_item.Tax;
                xferLineItem.TaxLines = line_item.tax_lines.ToFinAnalyzerTaxLines();
                FinAnalyzer.LineItems.Add(xferLineItem);
            }

            foreach (var refund in shopifyOrder.refunds)
            {
                var xferRefund = new FinAnalyzerRefund();
                xferRefund.ExternalRefNbr = refund.id.ToString();
                xferRefund.TaxableAmount = refund.TotalTaxableLineAndShippingAmount;
                xferRefund.LineItemTotal = refund.LineItemTotal;
                xferRefund.LineItemsTax = refund.TotalLineItemTax;
                xferRefund.Freight = refund.TotalShippingAdjustment;
                xferRefund.FreightTax = refund.TotalShippingAdjustmentTax;
                xferRefund.Credit = refund.CreditMemoTotal;
                xferRefund.Debit = refund.DebitMemoTotal;
                xferRefund.RefundAmount = refund.PaymentTotal;
                FinAnalyzer.Refunds.Add(xferRefund);
            }

            FinAnalyzer.Payment = orderRecord.ShopifyPaymentAmount();
            return FinAnalyzer;
        }

        public static List<FinAnalyzerTaxLine> ToFinAnalyzerTaxLines(this IEnumerable<TaxLine> taxLines)
        {
            return taxLines.Select(x => new FinAnalyzerTaxLine(x.title, x.rate)).ToList();
        }

        public static List<FinAnalyzerTaxLine> ToFinAnalyzerTaxLines(this IEnumerable<ShippingLine> shippingLines)
        {
            return shippingLines.Count() == 0
                ? new List<FinAnalyzerTaxLine>()
                : shippingLines.First().tax_lines.ToFinAnalyzerTaxLines();
        }
    }
}

