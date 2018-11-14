﻿using System;
using System.Data.Entity;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Shopify
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


        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
