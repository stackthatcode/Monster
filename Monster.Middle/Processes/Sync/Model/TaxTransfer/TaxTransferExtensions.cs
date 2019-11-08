﻿using System.Collections.Generic;
using System.Linq;
using Monster.TaxTransfer;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Model.TaxTransfer
{
    public static class TaxTransferExtensions
    {

        public static Transfer ToTaxTransfer(this Order shopifyOrder)
        {
            var transfer = new Transfer();
            transfer.ExternalRefNbr = shopifyOrder.id.ToString();

            transfer.Freight.Price = shopifyOrder.ShippingDiscountedTotal;
            transfer.Freight.IsTaxable = shopifyOrder.ShippingTotal > 0 && shopifyOrder.ShippingTax > 0;
            transfer.Freight.TaxLines = shopifyOrder.shipping_lines.ToTransferTaxLines();
            transfer.Freight.TaxAmount = shopifyOrder.ShippingTax;

            foreach (var line_item in shopifyOrder.line_items)
            {
                var xferLineItem = new TransferLineItem();
                xferLineItem.ExternalRefNbr = line_item.id.ToString();
                xferLineItem.InventoryID = line_item.sku;
                xferLineItem.Quantity = line_item.quantity;
                xferLineItem.UnitPrice = line_item.LineAmountAfterDiscount;
                xferLineItem.IsTaxable = line_item.taxable;
                xferLineItem.TaxAmount = line_item.Tax;
                xferLineItem.TaxLines = line_item.tax_lines.ToTransferTaxLines();

                transfer.LineItems.Add(xferLineItem);
            }

            foreach (var refund in shopifyOrder.refunds)
            {
                var xferRefund = new TransferRefund();
                xferRefund.ExternalRefNbr = refund.id.ToString();
                xferRefund.RefundAmount = refund.RefundTotal;
                xferRefund.FreightTax = refund.TotalShippingAdjustmentTax;
                xferRefund.TaxableFreightAmount = refund.TotalTaxableShippingAdjustment;
                xferRefund.TotalLineItemsTax = refund.TotalLineItemTax;
                xferRefund.TotalTaxableLineAmounts = refund.TotalTaxableLineItemAmount;

                transfer.Refunds.Add(xferRefund);
            }

            return transfer;
        }

        public static List<TransferTaxLine> ToTransferTaxLines(this IEnumerable<TaxLine> taxLines)
        {
            return taxLines.Select(x => new TransferTaxLine(x.title, x.rate)).ToList();
        }

        public static List<TransferTaxLine> ToTransferTaxLines(this IEnumerable<ShippingLine> shippingLines)
        {
            return shippingLines.Count() == 0
                ? new List<TransferTaxLine>()
                : shippingLines.First().tax_lines.ToTransferTaxLines();
        }
    }
}
