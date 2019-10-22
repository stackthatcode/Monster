using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Misc.State;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
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
        public List<ShopifyOrder> RetrieveShopifyOrdersNotSynced()
        {
            var preferences = Entities.Preferences.First();
            var orderNumberStart = preferences.StartingShopifyOrderId ?? 0;

            return Entities
                .ShopifyOrders
                .Where(x => x.ShopifyOrderId >= orderNumberStart
                            && !x.ShopAcuOrderSyncs.Any())
                .Include(x => x.ShopifyCustomer)
                .Include(x => x.ShopifyTransactions)
                .Include(x => x.ShopifyTransactions.Select(y => y.ShopifyAcuPayment))
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
        
        public ShopAcuOrderSync InsertOrderSync(
                ShopifyOrder shopifyOrder, AcumaticaSalesOrder acumaticaSalesOrder)
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

        public List<ShopifyCustomer> RetrieveUnsyncedShopifyCustomers()
        {
            return Entities
                .ShopifyCustomers
                .Where(x => !x.ShopAcuCustomerSyncs.Any())
                .Where(x => Entities.ShopifyOrders.Any(
                    y => y.ShopifyCustomer.ShopifyCustomerId == x.ShopifyCustomerId))
                .ToList();
        }

        public List<ShopifyCustomer> RetrieveCustomersNeedingUpdate()
        {
            return Entities
                .ShopifyCustomers
                .Where(x => x.ShopAcuCustomerSyncs.Any() &&
                            x.IsUpdatedInAcumatica == false)
                .ToList();
        }

        public void InsertCustomerSync(ShopAcuCustomerSync input)
        {
            Entities.ShopAcuCustomerSyncs.Add(input);
            Entities.SaveChanges();
        }



        // Shopify Fulfillments
        //
        public List<ShopifyFulfillment> RetrieveFulfillmentsNotSynced()
        {
            return Entities
                .ShopifyFulfillments
                .Include(x => x.ShopifyOrder)
                .Include(x => x.ShopifyOrder.ShopAcuOrderSyncs)
                .Include(x => x.ShopifyOrder.ShopAcuOrderSyncs.Select(y => y.AcumaticaSalesOrder))
                .Where(x => !x.ShopAcuShipmentSyncs.Any())
                .ToList();
        }

        public List<ShopifyFulfillment> RetrieveFulfillment(long shopifyOrderId)
        {
            return Entities
                    .ShopifyFulfillments
                    .Include(x => x.ShopAcuShipmentSyncs)
                    .Where(x => x.ShopifyOrderId == shopifyOrderId)
                    .ToList();
        }

        public bool AnyUnsyncedFulfillments(long shopifyOrderId)
        {
            return Entities
                .ShopifyFulfillments
                .Any(x => !x.ShopAcuShipmentSyncs.Any());
        }
        

        public void InsertShipmentDetailSync(
                ShopifyFulfillment fulfillment, 
                AcumaticaShipmentSalesOrderRef detail)
        {
            var sync = new ShopAcuShipmentSync();
            sync.ShopifyFulfillment = fulfillment;
            sync.AcumaticaShipmentSalesOrderRef = detail;
            sync.DateCreated = DateTime.UtcNow;
            sync.LastUpdated = DateTime.UtcNow;
            Entities.ShopAcuShipmentSyncs.Add(sync);
            Entities.SaveChanges();
        }
        


        // Shopify Refunds
        //
        public List<ShopifyRefund> RetrieveReturnsNotSynced()
        {
            return RefundRecordGraph
                    .Where(x => x.ShopifyOrder.ShopAcuOrderSyncs.Any())
                    .Where(x => x.ShopifyIsCancellation == false)
                    .Where(x => x.ShopAcuRefundCms.Any() == false ||
                                x.ShopAcuRefundCms.Any(y => y.IsComplete == false))
                    .ToList();
        }

        public ShopifyRefund RetrieveRefundAndSync(long shopifyRefundId)
        {
            return Entities
                .ShopifyRefunds
                .Include(x => x.ShopAcuRefundCms)
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
        
        public void InsertRefundSync(ShopAcuRefundCm input)
        {
            Entities.ShopAcuRefundCms.Add(input);
            Entities.SaveChanges();
        }


        // Shopify Transactions
        //
        public List<ShopifyTransaction> RetrieveUnsyncedPayments()
        {
            return Entities.ShopifyTransactions
                .Include(x => x.ShopifyOrder)
                .Include(x => x.ShopifyOrder.ShopifyCustomer)
                .Where(x => x.ShopifyOrder.ShopAcuOrderSyncs.Any()
                            && x.ShopifyGateway != Gateway.Manual
                            && x.ShopifyStatus == TransactionStatus.Success
                            && (x.ShopifyKind == TransactionKind.Capture
                                || x.ShopifyKind == TransactionKind.Sale
                                || x.ShopifyKind == TransactionKind.Refund)
                            && x.ShopifyAcuPayment == null)
                .ToList();
        }


        // NOTE - this does not indicate whether the Credit Memo and Credit Memo Invoice
        // ... have yet been created for this Refund - we'll need to pull the Order Refund
        // ... and its sync record for that
        public List<ShopifyTransaction> RetrieveUnsyncedRefunds()
        {
            return Entities.ShopifyTransactions
                .Include(x => x.ShopifyOrder)
                .Include(x => x.ShopifyOrder.ShopifyCustomer)
                .Where(x => x.ShopifyOrder.ShopAcuOrderSyncs.Any()
                            && x.ShopifyGateway != Gateway.Manual
                            && x.ShopifyStatus == TransactionStatus.Success
                            && x.ShopifyKind == TransactionKind.Refund
                            && x.ShopifyRefundId != null
                            && x.ShopifyAcuPayment == null)
                .ToList();
        }
        


        public void InsertPayment(ShopifyAcuPayment payment)
        {
            Entities.ShopifyAcuPayments.Add(payment);
            Entities.SaveChanges();
        }




        public List<OrderSummaryViewDto> RetrieveOrderSyncView()
        {
            var sql = 
                @"SELECT ShopifyOrderId, ShopifyOrderNumber, AcumaticaOrderNbr, AcumaticaInvoiceNbr, AcumaticaShipmentNbr
                FROM vw_SyncOrdersAndSalesOrders
                WHERE ShopifyOrderId IS NOT NULL
                ORDER BY ShopifyOrderId DESC";

            return Entities
                    .Database
                    .SqlQuery<OrderSummaryViewDto>(sql)
                    .ToList();
        }
        
        public int RetrieveTotalOrders()
        {
            var sql = "SELECT COUNT(*) FROM vw_SyncOrdersAndSalesOrders WHERE ShopifyOrderId IS NOT NULL;";
            return Entities.ScalarQuery<int>(sql);
        }

        public int RetrieveTotalOrdersSynced()
        {
            var sql =
                @"SELECT COUNT(*) FROM vw_SyncOrdersAndSalesOrders 
                WHERE ShopifyOrderId IS NOT NULL 
                AND AcumaticaOrderNbr IS NOT NULL";
            return Entities.ScalarQuery<int>(sql);
        }

        public int RetrieveTotalOrdersOnShipments()
        {
            var sql =
                @"SELECT COUNT(*) FROM vw_SyncOrdersAndSalesOrders 
                WHERE ShopifyOrderId IS NOT NULL 
                AND AcumaticaOrderNbr IS NOT NULL
                AND AcumaticaShipmentNbr IS NOT NULL;";
            return Entities.ScalarQuery<int>(sql);
        }

        public int RetrieveTotalOrdersInvoiced()
        {
            var sql =
                @"SELECT COUNT(*) FROM vw_SyncOrdersAndSalesOrders 
                WHERE ShopifyOrderId IS NOT NULL 
                AND AcumaticaOrderNbr IS NOT NULL
                AND AcumaticaShipmentNbr IS NOT NULL
                AND AcumaticaInvoiceNbr IS NOT NULL;";
            return Entities.ScalarQuery<int>(sql);
        }
        


        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
