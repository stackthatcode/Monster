using System.Collections.Generic;

namespace Push.Shopify.Legacy.Order
{
    public class Deserializer
    {
        public Order DeserializeOrder(dynamic order)
        {
            var orderResult = new Order
                {
                    Id = order.id,
                    Email = order.email,
                    Name = order.name,
                    SubTotal = order.subtotal_price,
                    TotalTax = order.total_tax,
                    CreatedAt = order.processed_at,
                    UpdatedAt = order.updated_at,
                    LineItems = new List<OrderLineItem>(),
                    AllRefunds = new List<Refund>(),
                    OrderDiscount = 0.00m,
                    CancelledAt = order.cancelled_at,
                    FinancialStatus = order.financial_status,
                    Tags = order.tags,
                };

            // The Order Discount is just the sum of all Discount Codes
            foreach (var discount_code in order.discount_codes)
            {
                if (discount_code.type != "shipping")
                {
                    decimal amount = discount_code.amount;
                    orderResult.OrderDiscount += amount;
                }
            }

            foreach (var line_item in order.line_items)
            {
                var orderLineItemResult = new OrderLineItem();

                orderLineItemResult.Id = line_item.id;
                orderLineItemResult.Discount = line_item.total_discount;
                orderLineItemResult.ProductId = line_item.product_id;
                orderLineItemResult.VariantId = line_item.variant_id;
                orderLineItemResult.Price = line_item.price;
                orderLineItemResult.IsGiftCard = line_item.gift_card;
                orderLineItemResult.Quantity = line_item.quantity;
                orderLineItemResult.Sku = line_item.sku;

                orderLineItemResult.ProductTitle = line_item.title;
                orderLineItemResult.VariantTitle = line_item.variant_title;
                orderLineItemResult.Name = line_item.name;
                orderLineItemResult.Vendor = line_item.vendor;
                orderLineItemResult.ParentOrder = orderResult;

                orderResult.LineItems.Add(orderLineItemResult);
            }

            foreach (var refund in order.refunds)
            {
                var refundResult = new Refund();
                refundResult.ParentOrder = orderResult;
                refundResult.Id = refund.id;
                refundResult.CreatedAtShopTz = refund.created_at;
                refundResult.LineItems = new List<RefundLineItem>();
                refundResult.OrderAdjustments = new List<OrderAdjustment>();
                refundResult.Transactions = new List<Transaction>();

                foreach (var refundLineItems in refund.refund_line_items)
                {
                    var resultRefundLineItem = new RefundLineItem();

                    resultRefundLineItem.Id = refundLineItems.id;
                    resultRefundLineItem.ParentRefund = refundResult;
                    resultRefundLineItem.LineItemId = refundLineItems.line_item_id;
                    resultRefundLineItem.RestockQuantity = refundLineItems.quantity;
                    resultRefundLineItem.TaxTotal = refundLineItems.total_tax;
                    resultRefundLineItem.SubTotal = refundLineItems.subtotal;

                    refundResult.LineItems.Add(resultRefundLineItem);
                }

                foreach (var adjustment in refund.order_adjustments)
                {
                    var adjustmentItem = new OrderAdjustment();

                    adjustmentItem.Refund = refundResult;
                    adjustmentItem.Id = adjustment.id;
                    adjustmentItem.Amount = adjustment.amount;
                    adjustmentItem.Kind = adjustment.kind;
                    adjustmentItem.Reason = adjustment.reason;
                    adjustmentItem.TaxAmount = adjustment.tax_amount;

                    refundResult.OrderAdjustments.Add(adjustmentItem);
                }

                foreach (var transaction in refund.transactions)
                {
                    var transactionItem = new Transaction();
                    transactionItem.Id = transaction.id;
                    transactionItem.Amount = transaction.amount;
                    transactionItem.Status = transaction.status;
                    refundResult.Transactions.Add(transactionItem);
                }

                orderResult.AllRefunds.Add(refundResult);
            }

            return orderResult;
        }

    }
}
