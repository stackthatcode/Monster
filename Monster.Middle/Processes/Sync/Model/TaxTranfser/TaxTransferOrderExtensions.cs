using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.TaxTransfer.v2;
using Push.Foundation.Utilities.General;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Model.TaxTranfser
{
    public static class TaxTransferOrderExtensions
    {
        public static string ToSerializedAndZippedTaxTransfer(this ShopifyOrder orderRecord)
        {
            return orderRecord.ToShopifyObj().ToSerializedAndZippedTaxTransfer();
        }

        public static string ToSerializedAndZippedTaxTransfer(this Order order)
        {
            return order.ToTaxTransfer().Serialize().ToBase64Zip();
        }

        public static TaxTransfer.v2.TaxTransfer ToTaxTransfer(this ShopifyOrder shopifyOrderRecord)
        {
            return shopifyOrderRecord.ToShopifyObj().ToTaxTransfer();
        }

        public static TaxTransfer.v2.TaxTransfer ToTaxTransfer(this Order shopifyOrder)
        {
            var snapshot = new TaxTransfer.v2.TaxTransfer();

            snapshot.ShopifyOrderId = shopifyOrder.id;
            snapshot.ShopifyRefundIds = shopifyOrder.refunds.Select(x => x.id).OrderBy(x => x).ToList();

            snapshot.NetTaxableFreight = shopifyOrder.NetShippingTaxableTotal;
            snapshot.NetFreightTax = shopifyOrder.NetShippingTax;

            // *** Includes Shipping
            //
            snapshot.NetTaxableAmount =
                shopifyOrder.line_items.Sum(x => x.TaxableAmount)
                  + shopifyOrder.ShippingTaxableTotal
                  - shopifyOrder.refunds.Sum(x => x.TotalTaxableLineAndShippingAmount);
            snapshot.NetTotalTax = shopifyOrder.NetLineItemTax + shopifyOrder.NetShippingTax;

            snapshot.LineItems = new List<TaxTransferLineItem>();

            foreach (var line_item in shopifyOrder.line_items)
            {
                var snapshot_line = new TaxTransferLineItem();
                snapshot_line.ItemID = line_item.sku;
                snapshot_line.TaxLines = line_item.tax_lines.ToSnapshotTaxLines();
                snapshot.LineItems.Add(snapshot_line);
            }

            snapshot.FreightTaxLines
                = shopifyOrder
                    .shipping_lines
                    .SelectMany(x => x.tax_lines)
                    .ToSnapshotTaxLines();

            return snapshot;
        }

        public static List<TaxTransferTaxLine> ToSnapshotTaxLines(this IEnumerable<TaxLine> taxLines)
        {
            return taxLines.Select(x => new TaxTransferTaxLine(x.title, x.rate)).ToList();
        }
    }
}
