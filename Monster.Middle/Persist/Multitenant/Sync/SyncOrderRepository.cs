using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Sync
{
    public class SyncOrderRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public SyncOrderRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        public List<UsrShopifyOrder> RetrieveShopifyOrdersNotSynced()
        {
            var preferences = Entities.UsrPreferences.First();
            
            return Entities
                .UsrShopifyOrders
                .Where(x => x.ShopifyOrderNumber >= preferences.ShopifyOrderPushStart 
                            && !x.UsrShopAcuOrderSyncs.Any())
                .Include(x => x.UsrShopifyCustomer)
                .ToList();
        }
        
        
        public UsrShopifyOrder RetrieveShopifyOrder(long shopifyOrderId)
        {
            return Entities
                .UsrShopifyOrders
                .Include(x => x.UsrShopAcuOrderSyncs)
                .Include(x => x.UsrShopAcuOrderSyncs.Select(y => y.UsrAcumaticaSalesOrder))
                .Include(x => x.UsrShopAcuOrderSyncs.Select(y => y.UsrAcumaticaSalesOrder.UsrAcumaticaCustomer))
                .FirstOrDefault(x => x.ShopifyOrderId == shopifyOrderId);
        }

        public UsrAcumaticaSalesOrder RetrieveSalesOrder(string orderNbr)
        {
            return Entities
                .UsrAcumaticaSalesOrders
                .Include(x => x.UsrShopAcuOrderSyncs)
                .Include(x => x.UsrAcumaticaCustomer)
                .Include(x => x.UsrShopAcuOrderSyncs.Select(y => y.UsrShopifyOrder))
                .Include(x => x.UsrShopAcuOrderSyncs.Select(y => y.UsrShopifyOrder.UsrShopifyCustomer))
                .FirstOrDefault(x => x.AcumaticaOrderNbr == orderNbr);
        }

        public UsrShopAcuOrderSync 
                InsertOrderSync(
                    UsrShopifyOrder shopifyOrder, 
                    UsrAcumaticaSalesOrder acumaticaSalesOrder)
        {
            var sync = new UsrShopAcuOrderSync();
            sync.UsrShopifyOrder = shopifyOrder;
            sync.UsrAcumaticaSalesOrder = acumaticaSalesOrder;
            sync.DateCreated = DateTime.UtcNow;
            sync.LastUpdated = DateTime.UtcNow;
            Entities.UsrShopAcuOrderSyncs.Add(sync);
            Entities.SaveChanges();
            return sync;
        }
        


        // Customer syncing
        //
        public UsrShopifyCustomer RetrieveCustomer(long shopifyCustomerId)
        {
            return Entities
                .UsrShopifyCustomers
                .Include(x => x.UsrShopAcuCustomerSyncs)
                .Include(x => x.UsrShopAcuCustomerSyncs.Select(y => y.UsrAcumaticaCustomer))
                .FirstOrDefault(x => x.ShopifyCustomerId == shopifyCustomerId);
        }

        public List<UsrShopifyCustomer> RetrieveCustomersUnsynced()
        {
            return Entities
                .UsrShopifyCustomers
                .Where(x => !x.UsrShopAcuCustomerSyncs.Any())
                .ToList();
        }
        
        public UsrShopAcuCustomerSync 
                InsertCustomerSync(
                    UsrShopifyCustomer shopifyCustomer, 
                    UsrAcumaticaCustomer acumaticaCustomer)
        {
            var sync = new UsrShopAcuCustomerSync();
            sync.UsrShopifyCustomer = shopifyCustomer;
            sync.UsrAcumaticaCustomer = acumaticaCustomer;
            sync.DateCreated = DateTime.UtcNow;
            sync.LastUpdated = DateTime.UtcNow;
            Entities.UsrShopAcuCustomerSyncs.Add(sync);
            Entities.SaveChanges();
            return sync;
        }



        // Shopify Fulfillments
        //
        public List<UsrShopifyFulfillment> RetrieveFulfillmentsNotSynced()
        {
            return Entities
                    .UsrShopifyFulfillments
                    .Include(x => x.UsrShopifyOrder)
                    .Include(x => x.UsrShopifyOrder.UsrShopAcuOrderSyncs)
                    .Include(x => x.UsrShopifyOrder.UsrShopAcuOrderSyncs.Select(y => y.UsrAcumaticaSalesOrder))
                    .Where(x => !x.UsrShopAcuShipmentSyncs.Any())
                    .ToList();
        }

        public void InsertShipmentDetailSync(
                UsrShopifyFulfillment fulfillment, 
                UsrAcumaticaShipmentDetail detail)
        {
            var sync = new UsrShopAcuShipmentSync();
            sync.UsrShopifyFulfillment = fulfillment;
            sync.UsrAcumaticaShipmentDetail = detail;
            sync.DateCreated = DateTime.UtcNow;
            sync.LastUpdated = DateTime.UtcNow;
            Entities.UsrShopAcuShipmentSyncs.Add(sync);
            Entities.SaveChanges();
        }


        public List<UsrAcumaticaShipment> 
                        RetrieveShipmentsByMonsterNotConfirmed()
        {
            return Entities
                .UsrAcumaticaShipments
                .Where(x => x.IsCreatedByMonster
                            && x.AcumaticaStatus == "Open")
                .ToList();
        }

        public List<UsrAcumaticaShipment> 
                        RetrieveShipmentsByMonsterWithNoInvoice()
        {
            return Entities
                .UsrAcumaticaShipments
                .Where(x => x.IsCreatedByMonster 
                            && x.AcumaticaInvoiceRefNbr == null
                            && x.AcumaticaStatus == "Confirmed")
                .ToList();
        }


        // Shopify Refunds
        //
        public List<UsrShopifyRefund> RetrieveRefundsNotSynced()
        {
            return Entities
                    .UsrShopifyRefunds
                    .Include(x => x.UsrShopifyOrder)
                    .Include(x => x.UsrShopifyOrder.UsrShopAcuOrderSyncs)
                    .Include(x => x.UsrShopifyOrder
                                    .UsrShopAcuOrderSyncs.Select(y => y.UsrAcumaticaSalesOrder))
                    .Where(x => !x.UsrShopAcuRefundCms.Any()
                                && x.UsrShopifyOrder.UsrShopAcuOrderSyncs.Any())
                    .ToList();
        }

        public void InsertRefundSync(UsrShopAcuRefundCm input)
        {
            Entities.UsrShopAcuRefundCms.Add(input);
            Entities.SaveChanges();
        }

        
        // To use later...?
        // public void UpdateShipmentInvoiceRefNbr(string) { }
        //
    }
}
