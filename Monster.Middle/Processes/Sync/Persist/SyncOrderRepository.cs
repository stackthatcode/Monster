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
                .Where(x => x.AcumaticaSalesOrder == null || x.NeedsOrderPut)
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
                .Include(x => x.AcumaticaSalesOrder)
                .Include(x => x.AcumaticaSalesOrder.AcumaticaCustomer)
                .Include(x => x.ShopifyTransactions)
                .Include(x => x.ShopifyTransactions.Select(y => y.AcumaticaPayment))
                .FirstOrDefault(x => x.ShopifyOrderId == shopifyOrderId);
        }

        public ShopifyOrder RetrieveShopifyOrderForTransactionSync(long shopifyOrderId)
        {
            return Entities
                .ShopifyOrders
                .AsNoTracking()
                .Include(x => x.AcumaticaSalesOrder)
                .Include(x => x.ShopifyTransactions)
                .Include(x => x.ShopifyTransactions.Select(y => y.AcumaticaPayment))
                .FirstOrDefault(x => x.ShopifyOrderId == shopifyOrderId);
        }

        public AcumaticaSalesOrder RetrieveSalesOrder(string orderNbr)
        {
            return Entities
                .AcumaticaSalesOrders
                .Include(x => x.AcumaticaCustomer)
                .Include(x => x.ShopifyOrder)
                .Include(x => x.ShopifyOrder.ShopifyCustomer)
                .FirstOrDefault(x => x.AcumaticaOrderNbr == orderNbr);
        }
        

        // Customer syncing
        //
        public ShopifyCustomer RetrieveCustomer(long shopifyCustomerId)
        {
            return Entities
                .ShopifyCustomers
                .Include(x => x.AcumaticaCustomer)
                .FirstOrDefault(x => x.ShopifyCustomerId == shopifyCustomerId);
        }

        public List<ShopifyCustomer> RetrieveShopifyCustomersWithoutSyncs()
        {
            return Entities.ShopifyCustomers.Where(x => x.AcumaticaCustomer == null).ToList();
        }

        public List<ShopifyCustomer> RetrieveShopifyCustomersNeedingPut()
        {
            return Entities
                .ShopifyCustomers
                .Where(x => x.AcumaticaCustomer == null && x.NeedsCustomerPut == true)
                .ToList();
        }



        // Shipment syncing
        //
        public bool AnyUnsyncedFulfillments(long shopifyOrderId)
        {
            return Entities.ShopifyOrders
                .Any(x => x.ShopifyOrderId == shopifyOrderId &&
                          x.ShopifyFulfillments.Any(y => !y.AcumaticaSoShipments.Any()));
        }

        public List<AcumaticaSoShipment> RetrieveUnsyncedSoShipments()
        {
            return Entities.AcumaticaSoShipments
                .Include(x => x.AcumaticaSalesOrder)
                .Where(x => x.NeedShipmentGet == false && x.ShopifyFulfillment == null)
                .ToList();
        }

       

        // Shopify Transactions
        //
        public List<long> RetrieveOrdersWithUnsyncedTransactions()
        {
            return Entities.ShopifyTransactions
                .Include(x => x.ShopifyOrder)
                .Where(x => x.ShopifyOrder.AcumaticaSalesOrder != null
                            && (x.Ignore == false && x.NeedsPaymentPut == true ||
                                x.AcumaticaPayment != null && !x.AcumaticaPayment.IsReleased))
                .Select(x => x.ShopifyOrder.Id)
                .Distinct()
                .ToList();
        }


        // Payment Synchronization
        //
        public void InsertPayment(AcumaticaPayment payment)
        {
            Entities.AcumaticaPayments.Add(payment);
            Entities.SaveChanges();
        }

        public void PaymentIsReleased(long shopifyTransactionMonsterId)
        {
            var payment = Entities.AcumaticaPayments.First(x => x.ShopifyTransactionMonsterId == shopifyTransactionMonsterId);
            payment.IsReleased = true;
            Entities.SaveChanges();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
