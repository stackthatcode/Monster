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
        public List<Transactions.Transaction> transactions { get; set; }
        public List<OrderAdjustment> order_adjustments { get; set; }


        [JsonIgnore]
        public Order Parent { get; set; }


        // Computed properties
        //
        //[JsonIgnore]
        //public bool IsValid => transactions.Any() && transactions.All(x => x.IsSuccess);
        //

        public decimal RefundAmount => transactions.Where(x => x.IsSuccess).Sum(x => x.amount);

        public IEnumerable<OrderAdjustment> ShippingAdjustments 
                => order_adjustments.Where(x => x.IsShippingAdjustment);

        public IEnumerable<OrderAdjustment> RefundDiscrepancies 
                => order_adjustments.Where(x => x.IsRefundDiscrepancy);

        [JsonIgnore]
        public decimal TotalTaxableShippingAdjustment => 
            -(ShippingAdjustments
                .Where(x => x.IsTaxable)
                .Sum(x => x.amount));

        [JsonIgnore]
        public decimal TotalShippingAdjustmentTax =>
            -(ShippingAdjustments
                .Where(x => x.IsTaxable)
                .Sum(x => x.tax_amount));

        [JsonIgnore]
        public decimal TotalLineItemTax => refund_line_items.Sum(x => x.total_tax);

        [JsonIgnore]
        public decimal TotalTaxableLineItemAmount 
            => refund_line_items
                .Where(x => x.IsTaxable)
                .Sum(x => x.subtotal);

        [JsonIgnore]
        public List<RefundLineItem> CancelledLineItems =>
            refund_line_items.Where(x => x.restock_type == "cancel").ToList();

        [JsonIgnore]
        public List<RefundLineItem> Returns =>
            refund_line_items.Where(x => x.restock_type == "return").ToList();


        public bool HasTransaction(long transaction_id)
        {
            return transactions.Any(x => x.id == transaction_id);
        }
    }
}

