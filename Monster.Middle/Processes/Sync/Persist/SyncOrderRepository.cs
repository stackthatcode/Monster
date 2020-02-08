using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Misc;


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
        public List<long> RetrieveShopifyOrdersToPut()
        {
            var settings = Entities.MonsterSettings.First();
            var numberOfOrders = settings.MaxNumberOfOrders;

            var newOrders = Entities.ShopifyOrdersNeedingOrderCreates
                .Select(x => x.ShopifyOrderId)
                .Take(numberOfOrders)
                .ToList();

            var updatedOrders = Entities.ShopifyOrdersNeedingOrderUpdates
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
                .Include(x => x.AcumaticaSalesOrder.ShopifyOrder)
                .Where(x => x.NeedShipmentAndInvoiceGet == false && x.ShopifyFulfillment == null)
                .ToList();
        }

       

        // Shopify Transactions
        //
        public List<long> RetrieveOrdersWithPaymentsAndRefundsNeedingActions()
        {
            var output = Entities
                .ShopifyOrdersNeedingPaymentSyncs.Select(x => x.ShopifyOrderId).ToList();

            output.AddRange(Entities
                .ShopifyOrdersNeedingOriginalPaymentUpdates.Select(x => x.ShopifyOrderId).ToList());

            output.AddRange(Entities
                .ShopifyOrderNeedingRefundSyncs
                .Select(x => x.ShopifyOrderId).ToList());

            return output.Distinct().OrderBy(x => x).ToList();
        }


        public void UpdateShopifyRefundsNeedsPut(long shopifyOrderMonsterId, bool value)
        {
            var refunds = Entities.ShopifyRefunds.Where(x => x.ShopifyOrderMonsterId == shopifyOrderMonsterId).ToList();
            foreach (var refund in refunds)
            {
                refund.NeedOriginalPaymentPut = value;
            }

            Entities.SaveChanges();
        }

        public void IncreaseOrderErrorCount(long shopifyOrderId)
        {
            var order = Entities.ShopifyOrders.First(x => x.ShopifyOrderId == shopifyOrderId);
            order.ErrorCount += 1;
            if (order.ErrorCount >= SystemConsts.ErrorThreshold)
            {
                order.Ignore = true;
            }
            Entities.SaveChanges();
        }

        public void ResetOrderErrorCount(long shopifyOrderId)
        {
            var order = Entities.ShopifyOrders.First(x => x.ShopifyOrderId == shopifyOrderId);
            order.ErrorCount = 0;
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
                .AcumaticaPayments.First(x => x.ShopifyTransactionMonsterId == shopifyTransactionMonsterId);

            payment.NeedRelease = false;
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
            var memo = Entities.AcumaticaMemoes.First(x => x.ShopifyRefundMonsterId == shopifyRefundMonsterId);
            memo.NeedRelease = false;
            Entities.SaveChanges();
        }


        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
