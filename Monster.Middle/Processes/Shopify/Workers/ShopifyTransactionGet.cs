using System;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Shopify.Workers
{
    public class ShopifyTransactionGet
    {
        private readonly OrderApi _orderApi;
        private readonly ShopifyOrderRepository _orderRepository;
        private readonly ShopifyJsonService _shopifyJsonService;
        private readonly ExecutionLogService _logService;
        
        public ShopifyTransactionGet(
                ShopifyOrderRepository orderRepository, 
                ShopifyJsonService shopifyJsonService,
                ExecutionLogService logService,
                OrderApi orderApi)
        {
            _shopifyJsonService = shopifyJsonService;
            _orderRepository = orderRepository;
            _logService = logService;
            _orderApi = orderApi;
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
            var order = _shopifyJsonService.RetrieveOrder(orderRecord.ShopifyOrderId);
            
            foreach (var transaction in transactions.transactions)
            {
                var transactionRecord = _orderRepository.RetrieveTransaction(transaction.id);
                if (transactionRecord != null)
                {
                    transactionRecord.LastUpdated = DateTime.UtcNow;
                    _orderRepository.SaveChanges();
                    continue;
                }

                using (var dbTransaction = _orderRepository.BeginTransaction())
                {
                    var record = new ShopifyTransaction();

                    if (transaction.kind == TransactionKind.Refund)
                    {
                        record.IsSyncableToPayment = true;

                        var refund = order.RefundByTransaction(transaction.id);
                        if (refund != null)
                        {
                            record.ShopifyRefundId = refund.id;
                            record.IsPureReturn = refund.IsPureReturn;
                        }
                    }

                    if (transaction.kind == TransactionKind.Capture || transaction.kind == TransactionKind.Sale)
                    {
                        record.IsSyncableToPayment = true;
                    }

                    record.ShopifyOrderId = transaction.order_id;
                    record.ShopifyTransactionId = transaction.id;
                    record.ShopifyStatus = transaction.status;
                    record.ShopifyKind = transaction.kind;
                    record.ShopifyGateway = transaction.gateway;
                    record.ShopifyAmount = transaction.amount;
                    record.ShopifyOrderMonsterId = orderRecord.MonsterId;
                    record.DateCreated = DateTime.UtcNow;
                    record.LastUpdated = DateTime.UtcNow;

                    _logService.Log(LogBuilder.DetectedNewShopifyTransaction(record));
                    _orderRepository.InsertTransaction(record);

                    _shopifyJsonService.Upsert(
                        ShopifyJsonType.Transaction, transaction.id, transaction.SerializeToJson());
                    dbTransaction.Commit();
                }
            }

            orderRecord.NeedsTransactionGet = false;
            _orderRepository.SaveChanges();
        }
    }
}

