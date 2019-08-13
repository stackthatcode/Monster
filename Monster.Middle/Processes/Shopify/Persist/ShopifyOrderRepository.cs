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
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public ShopifyOrderRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }



        // Shopify Order persistence
        //
        public UsrShopifyOrder RetrieveOrder(long shopifyOrderId)
        {
            return Entities
                .UsrShopifyOrders
                .Include(x => x.UsrShopifyCustomer)
                .FirstOrDefault(x => x.ShopifyOrderId == shopifyOrderId);
        }

        public DateTime? RetrieveOrderMaxUpdatedDate()
        {
            if (Entities.UsrShopifyOrders.Any())
            {
                return Entities.UsrShopifyOrders
                            .Select(x => x.LastUpdated)
                            .Max();
            }
            else
            {
                return (DateTime?) null;
            }
        }
        
        public void InsertOrder(UsrShopifyOrder order)
        {
            Entities.UsrShopifyOrders.Add(order);
            Entities.SaveChanges();
        }
        



        // Shopify Customer persistence
        // 
        public UsrShopifyCustomer 
                    RetrieveCustomer(long shopifyCustomerId)
        {
            return Entities
                .UsrShopifyCustomers
                .FirstOrDefault(x => x.ShopifyCustomerId == shopifyCustomerId);
        }

        public DateTime? RetrieveCustomerMaxUpdatedDate()
        {
            if (Entities.UsrShopifyCustomers.Any())
            {
                return Entities.UsrShopifyCustomers
                    .Select(x => x.LastUpdated)
                    .Max();
            }
            else
            {
                return (DateTime?)null;
            }
        }
        
        public bool DoesShopifyCustomerExist(long shopifyCustomerId)
        {
            return Entities
                .UsrShopifyCustomers
                .Any(x => x.ShopifyCustomerId == shopifyCustomerId);
        }

        public void InsertCustomer(UsrShopifyCustomer customer)
        {
            Entities.UsrShopifyCustomers.Add(customer);
            Entities.SaveChanges();
        }



        public UsrShopifyFulfillment RetreiveFulfillment(long shopifyFulfillmentId)
        {
            return Entities
                .UsrShopifyFulfillments
                .Include(x => x.UsrShopifyOrder)
                .FirstOrDefault(x => x.ShopifyFulfillmentId == shopifyFulfillmentId);
        }

        public void InsertFulfillment(UsrShopifyFulfillment fulfillment)
        {
            Entities.UsrShopifyFulfillments.Add(fulfillment);
            Entities.SaveChanges();
        }
        
        public void InsertRefund(UsrShopifyRefund refund)
        {
            Entities.UsrShopifyRefunds.Add(refund);
            Entities.SaveChanges();
        }


        // Transactions
        public void ImprintTransactions(
                long orderMonsterId, List<UsrShopifyTransaction> transactions)
        {
            var existingRecords =
                Entities
                    .UsrShopifyTransactions
                    .Where(x => x.OrderMonsterId == orderMonsterId)
                    .ToList();
            
            foreach (var latest in transactions)
            {
                var existing = existingRecords.Match(latest);
                if (existing == null)
                {
                    latest.DateCreated = DateTime.UtcNow;
                    latest.LastUpdated = DateTime.UtcNow;

                    Entities.UsrShopifyTransactions.Add(latest);
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

            var order = Entities.UsrShopifyOrders.First(x => x.Id == orderMonsterId);
            order.AreTransactionsUpdated = true;

            Entities.SaveChanges();
        }

        public List<UsrShopifyOrder> RetrieveOrdersNeedingTransactionPull()
        {
            return Entities
                .UsrShopifyOrders
                .Where(x => x.AreTransactionsUpdated == false)
                .ToList();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
