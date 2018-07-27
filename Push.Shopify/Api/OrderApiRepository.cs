using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.HttpClient;
using Push.Shopify.Api.Order;
using Push.Shopify.HttpClient;


namespace Push.Shopify.Api
{
    public class OrderApiRepository
    {
        private readonly ShopifyRequestBuilder _requestFactory;
        private readonly ClientFacade _executionFacade;
        private readonly IPushLogger _logger;

        public OrderApiRepository(
                ClientFacade executionFacade,
                ShopifyRequestBuilder requestFactory,
                IPushLogger logger)
        {
            _executionFacade = executionFacade;            
            _requestFactory = requestFactory;
            _logger = logger;
        }



        public virtual int RetrieveCount(OrderFilter filter)
        {
            var url = "/admin/orders/count.json?" + filter.ToQueryStringBuilder();
            var request = _requestFactory.HttpGet(url);
            var clientResponse = _executionFacade.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var count = parent.count;
            return count;
        }

        public virtual IList<Order.Order> 
                    Retrieve(OrderFilter filter, int page = 1, int limit = 250)
        {
            var querystring
                = new QueryStringBuilder()
                    .Add("page", page)
                    .Add("limit", limit)
                    .Add(filter.ToQueryStringBuilder())
                    .ToString();

            var path = string.Format("/admin/orders.json?" + querystring);

            var request = _requestFactory.HttpGet(path);
            var clientResponse = _executionFacade.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var results = new List<Order.Order>();

            foreach (var order in parent.orders)
            {
                var orderIdCatcher = order.id;
                var orderResult = DeserializeOrder(order);
                results.Add(orderResult); 
            }

            return results;
        }

        public virtual Order.Order Retrieve(long orderId)
        {
            var path = $"/admin/orders/{orderId}.json";  
            var request = _requestFactory.HttpGet(path);
            var response = _executionFacade.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(response.Body);
            return DeserializeOrder(parent.order);
        }



        public Order.Order DeserializeOrder(dynamic order)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.Trace($"Dump of Order ID: {order.id}" +
                                Environment.NewLine +
                                JsonConvert.SerializeObject(order));
            }

            _logger.Trace($"Translating Order {order.name} ({order.id})");

            var orderResult
                = new Order.Order
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
        
        public void Insert(string orderJson)
        {
            var path = "/admin/orders.json";
            var request = _requestFactory.HttpPost(path, orderJson);
            var clientResponse = _executionFacade.ExecuteRequest(request);
        }

    }
}
