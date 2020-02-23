using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Push.Shopify.Api.Order
{
    public class OrderParent
    {
        public Order order { get; set; }
    }

    public class OrderList
    {
        public List<Order> orders { get; set; }
    }

    public class Order
    {
        public long id { get; set; }
        public int number { get; set; }
        public int order_number { get; set; }
        public string name { get; set; }
        public bool test { get; set; }

        public DateTimeOffset? closed_at { get; set; }
        public DateTimeOffset created_at { get; set; }
        public DateTimeOffset updated_at { get; set; }
        public DateTimeOffset? processed_at { get; set; }
        
        public string financial_status { get; set; }
        public DateTimeOffset? cancelled_at { get; set; }
        public string cancel_reason { get; set; }
        
        public string currency { get; set; }
        public decimal total_price { get; set; }
        public decimal subtotal_price { get; set; }

        public bool taxes_included { get; set; }
        public decimal total_tax { get; set; }
        public decimal total_discounts { get; set; }
        public decimal total_line_items_price { get; set; }
        
        public List<LineItem> line_items { get; set; }
        public List<TaxLine> tax_lines { get; set; }
        public List<ShippingLine> shipping_lines { get; set; }
        public List<Refund> refunds { get; set; }

        public PaymentDetails payment_details { get; set; }
        public List<string> payment_gateway_names { get; set; }
        public string processing_method { get; set; }

        public List<DiscountApplication> discount_applications { get; set; }
        public List<DiscountCode> discount_codes { get; set; }

        public long? location_id { get; set; }
        public long? user_id { get; set; }
        public string email { get; set; }
        public string contact_email { get; set; }
        public bool buyer_accepts_marketing { get; set; }
        public Customer.Customer customer { get; set; }
        public OrderAddress billing_address { get; set; }
        public OrderAddress shipping_address { get; set; }

        public string fulfillment_status { get; set; }
        public List<Fulfillment> fulfillments { get; set; }
        
        public ClientDetails client_details { get; set; }
        


        // Quantity Totals
        //
        [JsonIgnore]
        public int TotalQuantity => 
            line_items.Sum(x => x.quantity)
            - refunds.SelectMany(x => x.refund_line_items).Sum(x => x.quantity);


        [JsonIgnore]
        public decimal LineAmountTotalAfterDiscount => line_items.Sum(x => x.LineAmountAfterDiscount);


        // Shipping Totals
        //
        [JsonIgnore]
        public decimal ShippingTotal => shipping_lines.Sum(x => x.price);
        [JsonIgnore]
        public decimal ShippingTaxableTotal => IsShippingTaxable ? ShippingTotal : 0m;
        [JsonIgnore]
        public decimal ShippingDiscountedTotal => shipping_lines.Sum(x => x.ShippingAfterDiscount);

        [JsonIgnore]
        public decimal ShippingTax => shipping_lines.Sum(x => x.TotalTaxes);

        [JsonIgnore]
        public bool IsShippingTaxable => ShippingTotal > 0m && ShippingTax > 0m;
        [JsonIgnore]
        public decimal NetShippingPrice => ShippingDiscountedTotal - refunds.Sum(x => x.TotalShippingAdjustment);
        [JsonIgnore]
        public decimal NetShippingTaxablePrice => IsShippingTaxable ? NetShippingPrice : 0m;
        [JsonIgnore]
        public decimal NetShippingTax => ShippingTax - refunds.Sum(x => x.TotalShippingAdjustmentTax);
        

        // Line Item (Tax)
        //
        [JsonIgnore]
        public decimal NetLineItemTax => line_items.Sum(x => x.Tax) - refunds.Sum(x => x.TotalLineItemTax);


        // Refund Totals
        //
        [JsonIgnore]
        public decimal RefundLineItemTotal => refunds.Sum(x => x.LineItemTotal);
        [JsonIgnore]
        public decimal RefundShippingTotal => refunds.Sum(x => x.TotalShippingAdjustment);
        [JsonIgnore]
        public decimal RefundCreditTotal => refunds.Sum(x => x.CreditMemoTotal);
        [JsonIgnore]
        public decimal RefundDebitTotal => refunds.Sum(x => x.DebitMemoTotal);
        [JsonIgnore]
        public decimal RefundTotalTax => refunds.Sum(x => x.TotalTax);
        [JsonIgnore]
        public decimal RefundTotal
                => RefundLineItemTotal + RefundShippingTotal + RefundCreditTotal + 
                   RefundTotalTax - RefundDebitTotal;
        [JsonIgnore]
        public decimal RefundOverpayment => refunds.Sum(x => x.PaymentTotal) - RefundTotal;

        [JsonIgnore]
        public decimal LineItemAmountAfterDiscountAndRefund => LineAmountTotalAfterDiscount - RefundLineItemTotal;

        [JsonIgnore]
        public decimal NetTax => total_tax - RefundTotalTax;

        [JsonIgnore]
        public decimal NetOrderTotal => LineItemAmountAfterDiscountAndRefund + NetShippingPrice + NetTax;

        [JsonIgnore]
        public List<LineItem> LineItemsWithManualVariants => line_items.Where(x => x.variant_id == null).ToList();

        public LineItem LineItem(string sku)
        {
            return line_items.FirstOrDefault(x => x.sku == sku);
        }

        public LineItem LineItem(long id)
        {
            return line_items.FirstOrDefault(x => x.id == id);
        }
        
        public List<LineItem> FulfillmentLineItems(long line_item_id)
        {
            return fulfillments
                .SelectMany(x => x.line_items)
                .Where(x => x.id == line_item_id)
                .ToList();
        }


        [JsonIgnore]
        public int RefundedLineItems => refunds.Sum(x => x.refund_line_items.Sum(y => y.quantity));

        [JsonIgnore]
        public bool AreAllLineItemsRefunded => (line_items.Sum(x => x.quantity) - RefundedLineItems) == 0;

        [JsonIgnore]
        public int NetOrderedQuantity => line_items.Sum(x => x.NetOrderedQuantity);

        public bool IsCancelled => cancelled_at.HasValue;


        public Refund RefundByTransaction(long transaction_id)
        {
            return refunds.FirstOrDefault(x => x.HasTransaction(transaction_id));
        }

        public Order Initialize()
        {
            line_items.ForEach(x => x.Parent = this);
            discount_applications.ForEach(x => x.Parent = this);
            refunds.ForEach(x => x.Parent = this);
            return this;
        }

        public List<DiscountAllocation> FindAllocations(DiscountApplication application)
        {
            var index = discount_applications.IndexOf(application);

            if (application.target_type == DiscountTargetType.LineItem)
            {
                var output =
                    line_items
                        .SelectMany(x => x.discount_allocations)
                        .Where(x => x.discount_application_index == index)
                        .ToList();

                return output;
            }

            if (application.target_type == DiscountTargetType.ShippingLine)
            {
                var output =
                    shipping_lines
                        .SelectMany(x => x.discount_allocations)
                        .Where(x => x.discount_application_index == index)
                        .ToList();

                return output;
            }

            throw new ArgumentException($"Unrecognized target_type {application.target_type}");
        }
    }
}

