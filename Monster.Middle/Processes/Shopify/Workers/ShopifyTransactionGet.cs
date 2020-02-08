﻿using System;
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
        private readonly ExecutionLogService _logService;
        
        public ShopifyTransactionGet(
                OrderApi orderApi, 
                ShopifyOrderRepository orderRepository, 
                ExecutionLogService logService)
        {
            _orderApi = orderApi;
            _orderRepository = orderRepository;
            _logService = logService;
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
                    record.IsSyncableToPayment = true;

                    var refund = order.RefundByTransaction(transaction.id);
                    if (refund != null)
                    {
                        record.ShopifyRefundId = refund.id;
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
                record.ShopifyJson = transaction.SerializeToJson();
                record.ShopifyGateway = transaction.gateway;
                record.ShopifyAmount = transaction.amount;
                record.ShopifyOrderMonsterId = orderRecord.MonsterId;
                record.DateCreated = DateTime.UtcNow;
                record.LastUpdated = DateTime.UtcNow;

                _logService.Log(LogBuilder.DetectedNewShopifyTransaction(record));
                _orderRepository.InsertTransaction(record);
            }

            orderRecord.NeedsTransactionGet = false;
            _orderRepository.SaveChanges();
        }

    }
}

