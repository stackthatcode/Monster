using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant
{
    public class OrderRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public OrderRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }



        // Shopify persistence
        //
        public UsrShopifyOrder RetrieveShopifyOrder(long shopifyOrderId)
        {
            return Entities
                    .UsrShopifyOrders
                    .FirstOrDefault(x => x.ShopifyOrderId == shopifyOrderId);
        }
        
        public DateTime? RetrieveShopifyOrderMaxUpdatedDate()
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
        
        public void InsertShopifyOrder(UsrShopifyOrder order)
        {
            Entities.UsrShopifyOrders.Add(order);
            Entities.SaveChanges();
        }
        

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
