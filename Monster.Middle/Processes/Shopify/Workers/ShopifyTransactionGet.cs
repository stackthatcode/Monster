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

        public void RunOptional(long shopifyOrderId)
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
            var transactionRecords = new List<ShopifyTransaction>();
            var order = orderRecord.ToShopifyObj();

            foreach (var transaction in transactions.transactions)
            {
                var record = new ShopifyTransaction();

                if (transaction.kind == TransactionKind.Refund)
                {
                    var refund = order.RefundByTransaction(transaction.id);
                    record.ShopifyRefundId = refund.id;
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

                transactionRecords.Add(record);
            }

            _orderRepository.ImprintTransactions(orderRecord.Id, transactionRecords);

            orderRecord.NeedsTransactionGet = false;
            _orderRepository.SaveChanges();
        }
    }
}

