using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        public void InsertShopifyOrderLineItem(UsrShopifyOrderLineItem lineItem)
        {
            Entities.UsrShopifyOrderLineItems.Add(lineItem);
            Entities.SaveChanges();
        }

        public List<UsrShopifyOrder> RetrieveShopifyOrdersNotSynced()
        {
            return Entities
                    .UsrShopifyOrders
                    .Where(x => !x.UsrAcumaticaSalesOrders.Any())
                    .Include(x => x.UsrShopifyCustomer)
                    .Include(x => x.UsrShopifyCustomer.UsrAcumaticaCustomers)
                    .Include(x => x.UsrShopifyOrderLineItems)
                    .ToList();
        }


        public UsrShopifyCustomer 
                    RetrieveShopifyCustomer(long shopifyCustomerId)
        {
            return Entities
                .UsrShopifyCustomers
                //.Include(x => x.UsrShopifyCustomerAddresses)
                .FirstOrDefault(x => x.ShopifyCustomerId == shopifyCustomerId);
        }

        public DateTime? RetrieveShopifyCustomerMaxUpdatedDate()
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

        public List<UsrShopifyCustomer> RetrieveShopifyCustomersUnsynced()
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

        public void InsertShopifyCustomer(UsrShopifyCustomer customer)
        {
            Entities.UsrShopifyCustomers.Add(customer);
            Entities.SaveChanges();
        }


        public UsrAcumaticaCustomer 
                    RetrieveAcumaticaCustomer(string acumaticaCustomerId)
        {
            return Entities
                    .UsrAcumaticaCustomers
                    .FirstOrDefault(
                        x => x.AcumaticaCustomerId == acumaticaCustomerId);
        }

        public UsrAcumaticaCustomer
                RetrieveAcumaticaCustomerByEmail(string email)
        {
            return Entities
                .UsrAcumaticaCustomers
                .FirstOrDefault(
                    x => x.AcumaticaMainContactEmail == email);
        }
        


        public DateTime? RetrieveAcumaticaCustomerMaxUpdatedDate()
        {
            if (Entities.UsrAcumaticaCustomers.Any())
            {
                return Entities.UsrAcumaticaCustomers
                    .Select(x => x.LastUpdated)
                    .Max();
            }
            else
            {
                return (DateTime?)null;
            }
        }
        
        public void InsertAcumaticaCustomer(UsrAcumaticaCustomer customer)
        {
            Entities.UsrAcumaticaCustomers.Add(customer);
            Entities.SaveChanges();
        }



        public UsrAcumaticaSalesOrder
                RetrieveAcumaticaSalesOrder(string acumaticaSOId)
        {
            return Entities
                    .UsrAcumaticaSalesOrders
                .FirstOrDefault(x => x.AcumaticaSalesOrderId == acumaticaSOId);
        }

        public void InsertAcumaticaSalesOrder(UsrAcumaticaSalesOrder order)
        {
            Entities.UsrAcumaticaSalesOrders.Add(order);
            Entities.SaveChanges();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
