using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public class ShopifyOrderRepository
    {
        private readonly ProcessPersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public ShopifyOrderRepository(ProcessPersistContext dataContext)
        {
            _dataContext = dataContext;
        }



        // Shopify Order persistence
        //
        public ShopifyOrder RetrieveOrder(long shopifyOrderId)
        {
            return Entities
                .ShopifyOrders
                .Include(x => x.ShopifyCustomer)
                .FirstOrDefault(x => x.ShopifyOrderId == shopifyOrderId);
        }

        public void InsertOrder(ShopifyOrder order)
        {
            Entities.ShopifyOrders.Add(order);
            Entities.SaveChanges();
        }
        



        // Shopify Customer persistence
        // 
        public ShopifyCustomer 
                    RetrieveCustomer(long shopifyCustomerId)
        {
            return Entities
                .ShopifyCustomers
                .FirstOrDefault(x => x.ShopifyCustomerId == shopifyCustomerId);
        }

        public void InsertCustomer(ShopifyCustomer customer)
        {
            Entities.ShopifyCustomers.Add(customer);
            Entities.SaveChanges();
        }



        public ShopifyFulfillment RetreiveFulfillment(long shopifyFulfillmentId)
        {
            return Entities
                .ShopifyFulfillments
                .Include(x => x.ShopifyOrder)
                .FirstOrDefault(x => x.ShopifyFulfillmentId == shopifyFulfillmentId);
        }

        public void InsertFulfillment(ShopifyFulfillment fulfillment)
        {
            Entities.ShopifyFulfillments.Add(fulfillment);
            Entities.SaveChanges();
        }
        
        public void InsertRefund(ShopifyRefund refund)
        {
            Entities.ShopifyRefunds.Add(refund);
            Entities.SaveChanges();
        }


        // Transactions
        public void ImprintTransactions(long orderMonsterId, List<ShopifyTransaction> transactions)
        {
            var existingRecords =
                Entities
                    .ShopifyTransactions
                    .Where(x => x.OrderMonsterId == orderMonsterId)
                    .ToList();
            
            foreach (var transaction in transactions)
            {
                var existing = existingRecords.Match(transaction);
                if (existing == null)
                {
                    transaction.DateCreated = DateTime.UtcNow;
                    transaction.LastUpdated = DateTime.UtcNow;

                    Entities.ShopifyTransactions.Add(transaction);
                }
                else
                {
                    existing.ShopifyOrderId = transaction.ShopifyOrderId;
                    existing.ShopifyTransactionId = transaction.ShopifyTransactionId;
                    existing.ShopifyStatus = transaction.ShopifyStatus;
                    existing.ShopifyKind = transaction.ShopifyKind;
                    existing.ShopifyJson = transaction.ShopifyJson;
                    existing.OrderMonsterId = transaction.OrderMonsterId;
                    existing.LastUpdated = DateTime.UtcNow;
                }
            }

            var order = Entities.ShopifyOrders.First(x => x.Id == orderMonsterId);
            order.NeedsTransactionGet = false;

            Entities.SaveChanges();
        }

        public List<ShopifyOrder> RetrieveOrdersNeedingTransactionPull()
        {
            return Entities
                .ShopifyOrders
                .Where(x => x.NeedsTransactionGet)
                .ToList();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
