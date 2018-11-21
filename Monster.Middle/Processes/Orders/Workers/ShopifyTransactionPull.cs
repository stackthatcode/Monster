using System;
using System.Collections.Generic;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Shopify;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Customer;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Orders.Workers
{
    public class ShopifyTransactionPull
    {
        private readonly OrderApi _orderApi;
        private readonly ShopifyOrderRepository _orderRepository;
        
        public ShopifyTransactionPull(
                OrderApi orderApi,
                ShopifyOrderRepository orderRepository)
        {
            _orderApi = orderApi;
            _orderRepository = orderRepository;
        }


        public void RunAutomatic()
        {
            var orders = 
                _orderRepository
                    .RetrieveOrdersNeedingTransactionPull();

            foreach (var orderRecord in orders)
            {
                var transactionsJson = _orderApi.RetrieveTransactions(orderRecord.ShopifyOrderId);
                var transactions 
                    = transactionsJson.DeserializeFromJson<TransactionList>();

                var transactionRecords = new List<UsrShopifyTransaction>();

                foreach (var transaction in transactions.transactions)
                {
                    var record = new UsrShopifyTransaction();
                    record.ShopifyOrderId = transaction.order_id;
                    record.ShopifyTransactionId = transaction.id;
                    record.OrderMonsterId = orderRecord.Id;
                    record.ShopifyJson = transaction.SerializeToJson();

                    transactionRecords.Add(record);
                }

                _orderRepository
                    .ImprintTransactions(orderRecord.Id, transactionRecords);
            }
        }    
    }
}

