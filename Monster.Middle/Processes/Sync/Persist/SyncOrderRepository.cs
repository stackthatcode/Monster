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
        private IQueryable<ShopifyOrder> 
            OrdersForPutting =>
                Entities.ShopifyOrders
                    .Include(x => x.ShopifyCustomer)
                    .Include(x => x.ShopifyFulfillments)
                    .Include(x => x.ShopifyRefunds)
                    .Include(x => x.ShopifyTransactions);

        public List<long> RetrieveShopifyOrdersToPut()
        {
            var settings = Entities.MonsterSettings.First();
            var numberOfOrders = settings.MaxNumberOfOrders;

            var newOrders = OrdersForPutting
                .Where(x => x.AcumaticaSalesOrder == null)
                .OrderBy(x => x.ShopifyOrderId)
                .Take(numberOfOrders)
                .Select(x => x.ShopifyOrderId)
                .ToList();

            var updatedOrders = OrdersForPutting
                .Where(x => x.NeedsOrderPut == true && x.AcumaticaSalesOrder != null)
                .OrderBy(x => x.ShopifyOrderId)
                .Select(x => x.ShopifyOrderId)
                .ToList();

            var output = newOrders;
            output.AddRange(updatedOrders);
            return output.Distinct().OrderBy(x => x).ToList();
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

        public ShopifyOrder RetrieveShopifyOrderWithNoTracking(long shopifyOrderId)
        {
            return Entities
                .ShopifyOrders
                .AsNoTracking()
                .Include(x => x.AcumaticaSalesOrder)
                .Include(x => x.AcumaticaSalesOrder.AcumaticaSoShipments)
                .Include(x => x.AcumaticaSalesOrder.AcumaticaSoShipments.Select(y => y.ShopifyFulfillment))
                .Include(x => x.ShopifyTransactions)
                .Include(x => x.ShopifyTransactions.Select(y => y.AcumaticaPayment))
                .FirstOrDefault(x => x.ShopifyOrderId == shopifyOrderId);
        }

        public ShopifyTransaction RetrieveShopifyTransactionWithNoTracking(long shopifyTransactionId)
        {
            return Entities
                .ShopifyTransactions
                .AsNoTracking()
                .Include(x => x.ShopifyOrder)
                .FirstOrDefault(x => x.ShopifyTransactionId == shopifyTransactionId);
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

        public long MaxShopifyOrderId()
        {
            return Entities.ShopifyOrders.Max(x => x.ShopifyOrderId);
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
                .Where(x => x.Ignore == false &&
                            x.ShopifyOrder.AcumaticaSalesOrder != null
                            && (x.AcumaticaPayment == null || x.NeedsPaymentPut == true))
                .Select(x => x.ShopifyOrder.ShopifyOrderId)
                .Distinct()
                .ToList();
        }

        public List<long> RetrieveOrdersWithUnreleasedTransactions()
        {
            return Entities.ShopifyTransactions
                .Include(x => x.ShopifyOrder)
                .Where(x => x.AcumaticaPayment != null && x.AcumaticaPayment.IsReleased == false)
                .Select(x => x.ShopifyOrder.ShopifyOrderId)
                .Distinct()
                .ToList();
        }


        public List<ShopifyTransaction> RetrieveUnsyncedTransactions(long shopifyOrderId)
        {
            return Entities.ShopifyTransactions
                .Where(x => x.ShopifyOrderId == shopifyOrderId)
                .Where(x => x.Ignore && x.AcumaticaPayment == null)
                .ToList();
        }

        public void UpdateShopifyTransactionNeedsPut(long monsterId, bool value)
        {
            var transaction = Entities.ShopifyTransactions.First(x => x.Id == monsterId);
            transaction.NeedsPaymentPut = value;
            Entities.SaveChanges();
        }

        public void UpdateShopifyIsBlocked(long shopifyOrderId, bool isBlocked)
        {
            var order = Entities.ShopifyOrders.First(x => x.ShopifyOrderId == shopifyOrderId);
            order.IsBlocked = isBlocked;
            Entities.SaveChanges();
        }

        public void UpdateShopifyHasError(long shopifyOrderId, bool hasError)
        {
            var order = Entities.ShopifyOrders.First(x => x.ShopifyOrderId == shopifyOrderId);
            order.HasError = hasError;
            Entities.SaveChanges();
        }

        // Payment Synchronization
        //
        public void InsertPayment(AcumaticaPayment payment)
        {
            Entities.AcumaticaPayments.Add(payment);
            Entities.SaveChanges();
        }

        public AcumaticaPayment RetreivePayment(long shopifyTransactionMonsterId)
        {
            return Entities
                .AcumaticaPayments
                .FirstOrDefault(x => x.ShopifyTransactionMonsterId == shopifyTransactionMonsterId);
        }

        public void PaymentIsReleased(long shopifyTransactionMonsterId)
        {
            var payment = Entities
                .AcumaticaPayments
                .First(x => x.ShopifyTransactionMonsterId == shopifyTransactionMonsterId);

            payment.IsReleased = true;
            Entities.SaveChanges();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
