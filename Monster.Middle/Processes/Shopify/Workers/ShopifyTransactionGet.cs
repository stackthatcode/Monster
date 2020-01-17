using System;
using System.Collections.Generic;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Shopify.Workers
{
    public class ShopifyTransactionGet
    {
        private readonly OrderApi _orderApi;
        private readonly ShopifyOrderRepository _orderRepository;
        
        public ShopifyTransactionGet(OrderApi orderApi, ShopifyOrderRepository orderRepository)
        {
            _orderApi = orderApi;
            _orderRepository = orderRepository;
        }


        public void RunAutomatic()
        {
            var orders = _orderRepository.RetrieveOrdersNeedingTransactionPull();

            foreach (var orderRecord in orders)
            {
                PullTransactionsFromShopify(orderRecord);
            }
        }

        public void RunTransactionIfPullNeeded(long shopifyOrderId)
        {
            var order = _orderRepository.RetrieveOrder(shopifyOrderId);
            if (order.NeedsTransactionGet)
            {
                PullTransactionsFromShopify(order);
            }
        }

        private void PullTransactionsFromShopify(ShopifyOrder orderRecord)
        {
            var transactionsJson = _orderApi.RetrieveTransactions(orderRecord.ShopifyOrderId);
            var transactions = transactionsJson.DeserializeFromJson<TransactionList>();
            var order = orderRecord.ToShopifyObj();

            foreach (var transaction in transactions.transactions)
            {
                var transactionRecord = _orderRepository.RetrieveTransaction(transaction.id);

                if (transactionRecord != null)
                {
                    transactionRecord.LastUpdated = DateTime.UtcNow;
                    _orderRepository.SaveChanges();
                    continue;
                }

                var record = new ShopifyTransaction();

                if (transaction.kind == TransactionKind.Refund)
                {
                    var refund = order.RefundByTransaction(transaction.id);
                    if (refund != null)
                    {
                        record.ShopifyRefundId = refund.id;
                    }
                }

                record.ShopifyOrderId = transaction.order_id;
                record.ShopifyTransactionId = transaction.id;
                record.ShopifyStatus = transaction.status;
                record.ShopifyKind = transaction.kind;
                record.ShopifyJson = transaction.SerializeToJson();
                record.ShopifyGateway = transaction.gateway;
                record.ShopifyAmount = transaction.amount;
                record.Ignore = transaction.Ignore();
                record.NeedsPaymentPut = !transaction.Ignore();
                record.OrderMonsterId = orderRecord.Id;

                // IF this is a refund, we will need to update the Payment -> Amount Applied To Orders
                //
                if (record.DoNotIgnore() && record.IsRefund())
                {
                    orderRecord.PaymentTransaction().NeedsPaymentPut = true;
                }

                record.DateCreated = DateTime.UtcNow;
                record.LastUpdated = DateTime.UtcNow;
                _orderRepository.InsertTransaction(record);
            }

            orderRecord.NeedsTransactionGet = false;
            _orderRepository.SaveChanges();
        }

    }
}

