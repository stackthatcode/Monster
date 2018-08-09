using System;
using System.Collections.Generic;
using System.Linq;

namespace Push.Shopify.Api.Order
{
    public class Refund
    {
        public long id { get; set; }
        public long order_id { get; set; }
        public DateTime created_at { get; set; }
        public string note { get; set; }
        public long? user_id { get; set; }
        public DateTime processed_at { get; set; }
        public bool restock { get; set; }

        public List<RefundLineItem> refund_line_items { get; set; }
        public List<Transaction> transactions { get; set; }
        public List<OrderAdjustment> order_adjustments { get; set; }


        // Computed properties

        public bool IsValid => transactions.Any() && transactions.All(x => x.IsSuccess);

        public decimal TransactionTotal => transactions.Sum(x => x.amount);
        
        public decimal AmountTotal => order_adjustments.Sum(x => x.amount);

        public decimal TaxTotal => order_adjustments.Sum(x => x.tax_amount);

        public decimal ShippingAdjustmentSubTotal =>
                            order_adjustments
                                .Where(x => x.IsShippingAdjustment)
                                .Sum(x => x.SubTotal);

        public decimal RefundDiscrepancySubTotal =>
                            order_adjustments
                                .Where(x => x.IsRefundDiscrepancy)
                                .Sum(x => x.SubTotal);

        public decimal SubTotal => order_adjustments.Sum(x => x.SubTotal);
    }
}

