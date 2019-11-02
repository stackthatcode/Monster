using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Sync.Persist
{
    public class SyncOrderRepository
    {
        private readonly ProcessPersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public SyncOrderRepository(ProcessPersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        public DbContextTransaction BeginTransaction()
        {
            return Entities.Database.BeginTransaction();
        }

        // Order Syncing
        //
        public List<ShopifyOrder> RetrieveShopifyOrdersToPut()
        {
            return Entities
                .ShopifyOrders
                .Where(x => !x.ShopAcuOrderSyncs.Any() || x.NeedsOrderPut)
                .Include(x => x.ShopifyCustomer)
                .Include(x => x.ShopifyFulfillments)
                .Include(x => x.ShopifyRefunds)
                .Include(x => x.ShopifyTransactions)
                .ToList();
        }

        public ShopifyOrder RetrieveShopifyOrder(long shopifyOrderId)
        {
            return Entities
                .ShopifyOrders
                .Include(x => x.ShopAcuOrderSyncs)
                .Include(x => x.ShopAcuOrderSyncs.Select(y => y.AcumaticaSalesOrder))
                .Include(x => x.ShopAcuOrderSyncs.Select(y => y.AcumaticaSalesOrder.AcumaticaCustomer))
                .FirstOrDefault(x => x.ShopifyOrderId == shopifyOrderId);
        }

        public AcumaticaSalesOrder RetrieveSalesOrder(string orderNbr)
        {
            return Entities
                .AcumaticaSalesOrders
                .Include(x => x.ShopAcuOrderSyncs)
                .Include(x => x.AcumaticaCustomer)
                .Include(x => x.ShopAcuOrderSyncs.Select(y => y.ShopifyOrder))
                .Include(x => x.ShopAcuOrderSyncs.Select(y => y.ShopifyOrder.ShopifyCustomer))
                .FirstOrDefault(x => x.AcumaticaOrderNbr == orderNbr);
        }
        
        public ShopAcuOrderSync InsertOrderSync(ShopifyOrder shopifyOrder, AcumaticaSalesOrder acumaticaSalesOrder)
        {
            var sync = new ShopAcuOrderSync();
            sync.ShopifyOrder = shopifyOrder;
            sync.AcumaticaSalesOrder = acumaticaSalesOrder;
            sync.DateCreated = DateTime.UtcNow;
            sync.LastUpdated = DateTime.UtcNow;

            Entities.ShopAcuOrderSyncs.Add(sync);
            Entities.SaveChanges();
            return sync;
        }


        // Customer syncing
        //
        public ShopifyCustomer RetrieveCustomer(long shopifyCustomerId)
        {
            return Entities
                .ShopifyCustomers
                .Include(x => x.ShopAcuCustomerSyncs)
                .Include(x => x.ShopAcuCustomerSyncs.Select(y => y.AcumaticaCustomer))
                .FirstOrDefault(x => x.ShopifyCustomerId == shopifyCustomerId);
        }

        public List<ShopifyCustomer> RetrieveShopifyCustomersWithoutSyncs()
        {
            return Entities.ShopifyCustomers.Where(x => !x.ShopAcuCustomerSyncs.Any()).ToList();
        }

        public List<ShopifyCustomer> RetrieveShopifyCustomersNeedingPut()
        {
            return Entities
                .ShopifyCustomers
                .Where(x => x.ShopAcuCustomerSyncs.Any() && x.NeedsCustomerPut == true)
                .ToList();
        }

        public void InsertCustomerSync(ShopAcuCustomerSync input)
        {
            Entities.ShopAcuCustomerSyncs.Add(input);
            Entities.SaveChanges();
        }



        // Shipment syncing
        //
        public bool AnyUnsyncedFulfillments(long shopifyOrderId)
        {
            return Entities.ShopifyOrders
                .Any(x => x.ShopifyOrderId == shopifyOrderId &&
                          x.ShopifyFulfillments.Any(y => !y.ShopAcuShipmentSyncs.Any()));
        }

        public List<AcumaticaSoShipment> RetrieveUnsyncedSoShipments()
        {
            return Entities.AcumaticaSoShipments
                .Include(x => x.AcumaticaSalesOrder)
                .Where(x => x.NeedShipmentGet == false && !x.ShopAcuShipmentSyncs.Any())
                .ToList();
        }

        public void InsertShipmentSync(
                ShopifyFulfillment fulfillmentRecord, AcumaticaSoShipment salesOrderShipment)
        {
            var sync = new ShopAcuShipmentSync();
            sync.ShopifyFulfillment = fulfillmentRecord;
            sync.AcumaticaSoShipment = salesOrderShipment;
            sync.DateCreated = DateTime.UtcNow;
            sync.LastUpdated = DateTime.UtcNow;

            Entities.ShopAcuShipmentSyncs.Add(sync);
            Entities.SaveChanges();
        }

        // Refund Syncing
        //
        public ShopifyRefund RetrieveRefundAndSync(long shopifyRefundId)
        {
            return Entities
                .ShopifyRefunds
                .FirstOrDefault(x => x.ShopifyRefundId == shopifyRefundId);
        }

        public List<ShopifyRefund> RetrieveCancelsNotSynced()
        {
            return RefundRecordGraph
                .Where(x => x.ShopifyOrder.ShopAcuOrderSyncs.Any())
                .Where(x => x.ShopifyIsCancellation && !x.IsCancellationSynced)
                .ToList();
        }
        
        private IQueryable<ShopifyRefund> RefundRecordGraph => 
            Entities
                .ShopifyRefunds
                .Include(x => x.ShopifyOrder)
                .Include(x => x.ShopifyOrder.ShopAcuOrderSyncs)
                .Include(x => x.ShopifyOrder.ShopAcuOrderSyncs.Select(y => y.AcumaticaSalesOrder));
        


        // Shopify Transactions
        //
        public List<ShopifyTransaction> RetrieveUnsyncedTransactions()
        {
            return Entities.ShopifyTransactions
                .Include(x => x.ShopifyOrder)
                .Include(x => x.ShopifyOrder.ShopifyCustomer)
                .Where(x => x.ShopifyOrder.ShopAcuOrderSyncs.Any() 
                            && x.Ignore == false 
                            && x.NeedsPaymentPut == true)
                .ToList();
        }

        // NOTE - this does not indicate whether the Credit Memo and Credit Memo Invoice
        // ... have yet been created for this Refund - we'll need to pull the Order Refund
        // ... and its sync record for that
        //
        public List<ShopifyTransaction> RetrieveUnsyncedRefunds()
        {
            return Entities.ShopifyTransactions
                .Include(x => x.ShopifyOrder)
                .Include(x => x.ShopifyOrder.ShopifyCustomer)
                .Where(x => x.ShopifyOrder.ShopAcuOrderSyncs.Any()
                            && x.ShopifyKind != TransactionKind.Refund
                            && x.Ignore == false
                            && x.NeedsPaymentPut == true)
                .ToList();
        }
        
        // Payment Synchronization
        //
        public void InsertPayment(ShopifyAcuPayment payment)
        {
            Entities.ShopifyAcuPayments.Add(payment);
            Entities.SaveChanges();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
