using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Orders;
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
        private IQueryable<ShopifyOrder> OrdersForPutting(int errorThreshold = SystemConsts.ErrorThreshold)
        {
            return Entities.ShopifyOrders
                .WhereOrderSyncErrorsBelowThreshold(errorThreshold)
                .Include(x => x.ShopifyCustomer)
                .Include(x => x.ShopifyFulfillments)
                .Include(x => x.ShopifyRefunds)
                .Include(x => x.ShopifyTransactions);
        }

        public List<long> RetrieveShopifyOrdersToPut(int errorThreshold = SystemConsts.ErrorThreshold)
        {
            var settings = Entities.MonsterSettings.First();
            var numberOfOrders = settings.MaxNumberOfOrders;

            var newOrders = OrdersForPutting(errorThreshold)
                .Where(x => x.AcumaticaSalesOrder == null)
                .OrderBy(x => x.ShopifyOrderId)
                .Take(numberOfOrders)
                .Select(x => x.ShopifyOrderId)
                .ToList();

            var updatedOrders = OrdersForPutting(errorThreshold)
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
                .Include(x => x.ShopifyRefunds)
                .Include(x => x.ShopifyRefunds.Select(y => y.AcumaticaMemo))
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

        public ShopifyRefund RetrieveShopifyRefundWithNoTracking(long shopifyRefundId)
        {
            return Entities
                .ShopifyRefunds
                .Include(x => x.ShopifyOrder)
                .Include(x => x.ShopifyOrder.AcumaticaSalesOrder)
                .Include(x => x.ShopifyOrder.AcumaticaSalesOrder.AcumaticaCustomer)
                .AsNoTracking()
                .Include(x => x.ShopifyOrder)
                .FirstOrDefault(x => x.ShopifyRefundId == shopifyRefundId);
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

        public List<AcumaticaSoShipment> RetrieveUnsyncedSoShipments(int errorThreshold = SystemConsts.ErrorThreshold)
        {
            return Entities.AcumaticaSoShipments
                .Include(x => x.AcumaticaSalesOrder)
                .Include(x => x.AcumaticaSalesOrder.ShopifyOrder)
                .WhereShipmentSyncErrorsBelowThreshold(errorThreshold)
                .Where(x => x.NeedShipmentGet == false && x.ShopifyFulfillment == null)
                .ToList();
        }

       

        // Shopify Transactions
        //
        public List<long> RetrieveSyncedOrdersWithUnsyncedTransactions(int errorTheshold = 3)
        {
            return Entities.ShopifyOrders
                .Include(x => x.ShopifyTransactions)
                .WhereOrderSyncErrorsBelowThreshold(errorTheshold)
                .Where(x => x.AcumaticaSalesOrder != null)
                .Where(x => x.ShopifyTransactions.Any(y => !y.Ignore && (y.AcumaticaPayment == null || y.NeedsPaymentPut))
                        || x.ShopifyRefunds.Any(y => y.AcumaticaMemo == null))
                .Select(x => x.ShopifyOrderId)
                .Distinct()
                .ToList();
        }

        public List<long> RetrieveOrdersWithUnreleasedTransactions(int errorTheshold = 3)
        {
            return Entities.ShopifyOrders
                .Include(x => x.ShopifyTransactions)
                .WhereOrderSyncErrorsBelowThreshold(errorTheshold)
                .Where(x => 
                    x.ShopifyTransactions.Any(y => y.AcumaticaPayment != null && y.AcumaticaPayment.IsReleased == false) ||
                    x.ShopifyRefunds.Any(y => y.AcumaticaMemo != null && y.AcumaticaMemo.IsReleased == false)
                )
                .Select(x => x.ShopifyOrderId)
                .Distinct()
                .ToList();
        }


        public void IncreaseSoShipmentErrorCount(string invoiceNbr, string shipmentNbr)
        {
            var soShipment = Entities.
                AcumaticaSoShipments
                .First(x => x.AcumaticaInvoiceNbr == invoiceNbr &&
                            x.AcumaticaShipmentNbr == shipmentNbr);
            soShipment.PutErrorCount += 1;
            Entities.SaveChanges();
        }

        public void ResetSoShipmentErrorCount(string invoiceNbr, string shipmentNbr)
        {
            var soShipment = Entities.
                AcumaticaSoShipments
                .First(x => x.AcumaticaInvoiceNbr == invoiceNbr &&
                            x.AcumaticaShipmentNbr == shipmentNbr);
            soShipment.PutErrorCount = 0;
            Entities.SaveChanges();
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

        public void IncreaseOrderErrorCount(long shopifyOrderId)
        {
            var order = Entities.ShopifyOrders.First(x => x.ShopifyOrderId == shopifyOrderId);
            order.PutErrorCount += 1;
            Entities.SaveChanges();
        }

        public void ResetOrderErrorCount(long shopifyOrderId)
        {
            var order = Entities.ShopifyOrders.First(x => x.ShopifyOrderId == shopifyOrderId);
            order.PutErrorCount = 0;
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

        public void UpdatePaymentRecordRefNbr(long shopifyTransactionMonsterId, string paymentNbr)
        {
            var payment = RetreivePayment(shopifyTransactionMonsterId);
            payment.AcumaticaRefNbr = paymentNbr;
            Entities.SaveChanges();
        }

        public void DeleteErroneousPaymentRecord(long shopifyTransactionMonsterId)
        {
            var payment = RetreivePayment(shopifyTransactionMonsterId);
            Entities.AcumaticaPayments.Remove(payment);
            Entities.SaveChanges();
        }

        public void PaymentIsReleased(long shopifyTransactionMonsterId)
        {
            var payment = Entities
                .AcumaticaPayments
                .First(x => x.ShopifyTransactionMonsterId == shopifyTransactionMonsterId);

            payment.IsReleased = true;
            Entities.SaveChanges();
        }


        // Credit Memo synchronization
        //
        public void InsertMemo(AcumaticaMemo memo)
        {
            Entities.AcumaticaMemoes.Add(memo);
            Entities.SaveChanges();
        }

        public AcumaticaMemo RetreiveMemo(long shopifyRefundMonsterId)
        {
            return Entities
                .AcumaticaMemoes
                .FirstOrDefault(x => x.ShopifyRefundMonsterId == shopifyRefundMonsterId);
        }

        public void UpdateMemoRecordRefNbr(long shopifyRefundMonsterId, string documentNbr)
        {
            var refund = RetreiveMemo(shopifyRefundMonsterId);
            refund.AcumaticaRefNbr = documentNbr;
            Entities.SaveChanges();
        }

        public void DeleteMemoPaymentRecord(long shopifyRefundMonsterId)
        {
            var refund = RetreiveMemo(shopifyRefundMonsterId);
            Entities.AcumaticaMemoes.Remove(refund);
            Entities.SaveChanges();
        }

        public void MemoIsReleased(long shopifyRefundMonsterId)
        {
            var invoice = Entities
                .AcumaticaMemoes
                .First(x => x.ShopifyRefundMonsterId == shopifyRefundMonsterId);

            invoice.IsReleased = true;
            Entities.SaveChanges();
        }


        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
