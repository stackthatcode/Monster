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
        public void ImprintTransactions(
                long orderMonsterId, List<ShopifyTransaction> transactions)
        {
            var existingRecords =
                Entities
                    .ShopifyTransactions
                    .Where(x => x.OrderMonsterId == orderMonsterId)
                    .ToList();
            
            foreach (var latest in transactions)
            {
                var existing = existingRecords.Match(latest);
                if (existing == null)
                {
                    latest.DateCreated = DateTime.UtcNow;
                    latest.LastUpdated = DateTime.UtcNow;

                    Entities.ShopifyTransactions.Add(latest);
                }
                else
                {
                    existing.ShopifyOrderId = latest.ShopifyOrderId;
                    existing.ShopifyTransactionId = latest.ShopifyTransactionId;
                    existing.ShopifyStatus = latest.ShopifyStatus;
                    existing.ShopifyKind = latest.ShopifyKind;
                    existing.ShopifyJson = latest.ShopifyJson;
                    existing.OrderMonsterId = latest.OrderMonsterId;
                    existing.LastUpdated = DateTime.UtcNow;
                }
            }

            var order = Entities.ShopifyOrders.First(x => x.Id == orderMonsterId);
            order.AreTransactionsUpdated = true;

            Entities.SaveChanges();
        }

        public List<ShopifyOrder> RetrieveOrdersNeedingTransactionPull()
        {
            return Entities
                .ShopifyOrders
                .Where(x => x.AreTransactionsUpdated == false)
                .ToList();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
