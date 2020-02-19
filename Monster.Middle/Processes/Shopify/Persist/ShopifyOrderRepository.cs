using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

        public DbContextTransaction BeginTransaction()
        {
            return _dataContext.Entities.Database.BeginTransaction();
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
        //
        public List<ShopifyOrder> RetrieveOrdersNeedingTransactionPull()
        {
            return Entities
                .ShopifyOrders
                .Include(x => x.ShopifyRefunds)
                .Where(x => x.NeedsTransactionGet)
                .ToList();
        }

        public ShopifyTransaction RetrieveTransaction(long shopifyTransactionId)
        {
            return Entities.ShopifyTransactions.FirstOrDefault(x => x.ShopifyTransactionId == shopifyTransactionId);
        }

        public void InsertTransaction(ShopifyTransaction transaction)
        {
            Entities.ShopifyTransactions.Add(transaction);
            Entities.SaveChanges();
        }

        public void UpdateNeedsTranasactionGet(long orderMonsterId, bool value)
        {
            var order = Entities.ShopifyOrders.First(x => x.MonsterId == orderMonsterId);
            order.NeedsTransactionGet = false;
            Entities.SaveChanges();
        }


        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
