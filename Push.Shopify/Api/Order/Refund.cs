using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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


        [JsonIgnore]
        public Order Parent { get; set; }



        //
        // Computed properties
        //
        public bool IsValid => transactions.Any() && transactions.All(x => x.IsSuccess);

        public decimal TransactionTotal => transactions.Sum(x => x.amount);

        public decimal RefundLineItemTotal => refund_line_items.Sum(x => x.Total);
        
        public decimal ShippingAdjustmentTotal =>
                            order_adjustments
                                .Where(x => x.IsShippingAdjustment)
                                .Sum(x => x.Total);

        public decimal RefundDiscrepancyTotal =>
                            order_adjustments
                                .Where(x => x.IsRefundDiscrepancy)
                                .Sum(x => x.Total);
        
        public decimal Total => 
                RefundLineItemTotal + ShippingAdjustmentTotal + RefundDiscrepancyTotal;

        public decimal TaxTotal =>
                refund_line_items.Sum(x => x.total_tax) +
                order_adjustments.Sum(x => x.tax_amount);
        
        public List<RefundTaxLine> 
                TaxBreakdown => Parent
                        .tax_lines
                        .Select(x => new RefundTaxLine()
                                {
                                    Rate = x.rate,
                                    Title = x.title,
                                    Price = x.PercentOfTotalTaxes * this.TaxTotal,
                                })
                        .ToList();

    }
}

