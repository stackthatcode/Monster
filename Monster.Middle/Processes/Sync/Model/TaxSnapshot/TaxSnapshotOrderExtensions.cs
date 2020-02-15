using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.TaxTransfer.v2;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Model.TaxSnapshot
{
    public static class TaxSnapshotOrderExtensions
    {
        public static TaxTransfer.v2.TaxSnapshot ToTaxSnapshot(this ShopifyOrder shopifyOrderRecord)
        {
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();

            var snapshot = new TaxTransfer.v2.TaxSnapshot();

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

            snapshot.LineItems = new List<TaxSnapshotLineItem>();

            foreach (var line_item in shopifyOrder.line_items)
            {
                var snapshot_line = new TaxSnapshotLineItem();
                snapshot_line.ItemID = line_item.sku;
                snapshot_line.TaxLines = line_item.tax_lines.ToSnapshotTaxLines();
                snapshot.LineItems.Add(snapshot_line);
            }

            snapshot.FreightTaxLines = shopifyOrder.shipping_lines.First().tax_lines.ToSnapshotTaxLines();

            return snapshot;
        }

        public static List<TaxSnapshotTaxLine> ToSnapshotTaxLines(this IEnumerable<TaxLine> taxLines)
        {
            return taxLines.Select(x => new TaxSnapshotTaxLine(x.title, x.rate)).ToList();
        }

        public static List<TaxSnapshotTaxLine> ToSnapshotTaxLines(this IEnumerable<ShippingLine> shippingLines)
        {
            return !shippingLines.Any()
                ? new List<TaxSnapshotTaxLine>() : shippingLines.First().tax_lines.ToSnapshotTaxLines();
        }
    }
}
