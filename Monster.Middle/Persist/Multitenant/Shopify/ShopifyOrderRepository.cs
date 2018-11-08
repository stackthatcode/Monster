using System;
using System.Collections.Generic;
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
                    .Include(x => x.UsrShopifyOrderLineItems)
                    .Include(x => x.UsrShopifyFulfillments)
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

        public void InsertOrderLineItem(UsrShopifyOrderLineItem lineItem)
        {
            Entities.UsrShopifyOrderLineItems.Add(lineItem);
            Entities.SaveChanges();
        }

        public List<UsrShopifyOrder> RetrieveOrdersNotSynced()
        {
            return Entities
                .UsrShopifyOrders
                .Where(x => !x.UsrAcumaticaSalesOrders.Any())
                .Include(x => x.UsrShopifyCustomer)
                .Include(x => x.UsrShopifyCustomer.UsrAcumaticaCustomers)
                .Include(x => x.UsrShopifyOrderLineItems)
                .Include(x => x.UsrAcumaticaSalesOrders)    
                .ToList();
        }

        public void Refresh<T>(T instance) where T: class
        {
            Entities.Entry<T>(instance).Reload();
        }


        // Shopify Customer persistence
        // 
        public UsrShopifyCustomer 
                    RetrieveCustomer(long shopifyCustomerId)
        {
            return Entities
                .UsrShopifyCustomers
                //.Include(x => x.UsrShopifyCustomerAddresses)
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

        public List<UsrShopifyCustomer> RetrieveCustomersUnsynced()
        {
            return Entities
                .UsrShopifyCustomers
                .Where(x => !x.UsrAcumaticaCustomers.Any())
                .ToList();
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


        // Shopify Fulfillments
        public List<UsrShopifyFulfillment>
                    RetrieveFulfillmentsNotSynced()
        {
            return Entities
                .UsrShopifyFulfillments
                .Include(x => x.UsrShopifyOrder)
                .Where(x =>
                    x.UsrShopifyOrder.UsrAcumaticaSalesOrders.Any()
                    && !x.UsrAcumaticaShipments.Any())
                .ToList();
        }

        public UsrShopifyFulfillment RetreiveFulfillment(long shopifyFulfillmentId)
        {
            return Entities
                .UsrShopifyFulfillments
                .Include(x => x.UsrShopifyOrder)
                .FirstOrDefault(x => x.ShopifyFulfillmentId == shopifyFulfillmentId);
        }

        public void InsertFulfillments(UsrShopifyFulfillment fulfillment)
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
