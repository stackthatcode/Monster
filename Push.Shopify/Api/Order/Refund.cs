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
        public decimal PaymentTotal => transactions.Where(x => x.IsSuccess).Sum(x => x.amount);

        public decimal LineItemTotal => refund_line_items.Sum(x => x.subtotal);


        // Shipping Refunds/Adjustments
        //
        [JsonIgnore]
        public IEnumerable<OrderAdjustment> ShippingAdjustments 
                => order_adjustments.Where(x => x.IsShippingAdjustment);

        [JsonIgnore]
        public decimal TotalShippingAdjustment => -ShippingAdjustments.Sum(x => x.amount);

        [JsonIgnore]
        public decimal TotalShippingAdjustmentTax => -(ShippingAdjustments.Sum(x => x.tax_amount));


        // Refund Discrepancy Adjustments
        //
        [JsonIgnore]
        private decimal RefundDiscrepanciesTotal
                => order_adjustments.Where(x => x.IsRefundDiscrepancy).Sum(x => x.amount);

        [JsonIgnore] public decimal CreditMemoTotal 
                => RefundDiscrepanciesTotal < 0 ? -RefundDiscrepanciesTotal : 0m;

        [JsonIgnore]
        public decimal DebitMemoTotal => RefundDiscrepanciesTotal > 0 ? RefundDiscrepanciesTotal : 0m;


        // Tax 
        //
        [JsonIgnore]
        public decimal TaxableLineAmounts => refund_line_items.Where(x => x.IsTaxable).Sum(x => x.subtotal);

        [JsonIgnore]
        public decimal TaxableShippingAdj => TotalShippingAdjustmentTax > 0 ? TotalShippingAdjustment : 0m;

        [JsonIgnore]
        public decimal TotalTaxableLineAndShippingAmount => TaxableLineAmounts + TaxableShippingAdj;

        [JsonIgnore]
        public decimal TotalLineItemTax => refund_line_items.Sum(x => x.total_tax);

        [JsonIgnore]
        public decimal TotalTax => TotalShippingAdjustmentTax + TotalLineItemTax;


        public bool IsPureReturn => refund_line_items.All(x => x.restock_type == RestockType.Return);
            

        public bool HasTransaction(long transaction_id)
        {
            return transactions.Any(x => x.id == transaction_id);
        }
    }
}

