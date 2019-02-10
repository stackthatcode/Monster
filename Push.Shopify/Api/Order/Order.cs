using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Push.Foundation.Utilities.Json;

namespace Push.Shopify.Api.Order
{
    public class OrderParent
    {
        public Order order { get; set; }
    }

    public class Order
    {
        public long id { get; set; }
        public int number { get; set; }
        public int order_number { get; set; }
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
        public List<OrderTaxLine> tax_lines { get; set; }
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
        public BillingAddress billing_address { get; set; }
        public ShippingAddress shipping_address { get; set; }

        public string fulfillment_status { get; set; }
        public List<Fulfillment> fulfillments { get; set; }
        
        public ClientDetails client_details { get; set; }


        //
        // Computed properties
        //
        [JsonIgnore]
        public decimal TaxLinesTotal => tax_lines.Sum(x => x.price);

        [JsonIgnore]
        public decimal TaxableAmountTotal => line_items.Sum(x => x.TaxableAmount);

        [JsonIgnore]
        public decimal LineItemDiscountTotal
                        => discount_applications
                            .Where(x => x.target_type == DiscountTargetType.LineItem)
                            .Sum(x => x.TotalAllocations);

        [JsonIgnore]
        public decimal RefundTransactionTotal => refunds.Sum(x => x.TransactionTotal);

        [JsonIgnore]
        public decimal RefundTotal => refunds.Sum(x => x.Total);

        [JsonIgnore]
        public decimal RefundTaxTotal => refunds.Sum(x => x.TaxTotal);
        
        [JsonIgnore]
        public decimal ShippingTax => shipping_lines.Sum(x => x.TotalTaxes);

        [JsonIgnore]
        public decimal ShippingDiscountsTotal
                        => discount_applications
                            .Where(x => x.target_type == DiscountTargetType.ShippingLine)
                            .Sum(x => x.TotalAllocations);

        [JsonIgnore]
        public decimal ShippingTotal => shipping_lines.Sum(x => x.price);

        [JsonIgnore]
        public decimal ShippingDiscountedTotal => ShippingTotal - ShippingDiscountsTotal;

        public LineItem LineItem(string sku)
        {
            return line_items.FirstOrDefault(x => x.sku == sku);
        }

        public LineItem LineItem(long id)
        {
            return line_items.FirstOrDefault(x => x.id == id);
        }
        
        public Fulfillment Fulfillment(long id)
        {
            return fulfillments.FirstOrDefault(x => x.id == id);
        }

        public List<RefundLineItem> RefundLineItems(long line_item_id)
        {
            return refunds
                .SelectMany(x => x.refund_line_items)
                .Where(x => x.line_item_id == line_item_id)
                .ToList();
        }

        public Refund RefundByTransaction(long transaction_id)
        {
            return refunds.FirstOrDefault(x => x.HasTransaction(transaction_id));
        }


        public void Initialize()
        {
            line_items.ForEach(x => x.Parent = this);
            discount_applications.ForEach(x => x.Parent = this);
            refunds.ForEach(x => x.Parent = this);
            tax_lines.ForEach(x => x.Parent = this);
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

            throw new ArgumentException(
                    $"Unrecognized target_type {application.target_type}");
        }
    }

    public class OrderList
    {
        public List<Order> orders { get; set; }
    }

    public static class OrderExtensions
    {
        public static OrderList DeserializeToOrderList(this string input)
        {
            var output = input.DeserializeFromJson<OrderList>();
            foreach (var order in output.orders)
            {
                order.Initialize();
            }

            return output;
        }

        public static Order DeserializeToOrder(this string input)
        {
            var output = input.DeserializeFromJson<Order>();
            output.Initialize();
            return output;
        }

        public static OrderParent DeserializeToOrderParent(this string input)
        {
            var output = input.DeserializeFromJson<OrderParent>();
            output.order.Initialize();
            return output;
        }
    }
}

