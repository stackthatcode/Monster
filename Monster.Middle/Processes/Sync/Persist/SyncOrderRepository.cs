using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Tenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Model.Orders;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Sync.Persist
{
    public class SyncOrderRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public SyncOrderRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        public DbContextTransaction BeginTransaction()
        {
            return Entities.Database.BeginTransaction();
        }

        // Order Syncing
        //
        public List<UsrShopifyOrder> RetrieveShopifyOrdersNotSynced()
        {
            var preferences = Entities.UsrPreferences.First();
            var orderNumberStart = preferences.ShopifyOrderNumberStart ?? 0;

            return Entities
                .UsrShopifyOrders
                .Where(x => x.ShopifyOrderNumber >= orderNumberStart
                            && !x.UsrShopAcuOrderSyncs.Any())
                .Include(x => x.UsrShopifyCustomer)
                .Include(x => x.UsrShopifyTransactions)
                .Include(x => x.UsrShopifyTransactions.Select(y => y.UsrShopifyAcuPayment))
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
                UsrAcumaticaSalesOrder acumaticaSalesOrder,
                string taxDetailId,
                bool isTaxLoadedToAcumatica)
        {
            var sync = new UsrShopAcuOrderSync();
            sync.UsrShopifyOrder = shopifyOrder;
            sync.UsrAcumaticaSalesOrder = acumaticaSalesOrder;
            sync.DateCreated = DateTime.UtcNow;
            sync.LastUpdated = DateTime.UtcNow;
            sync.AcumaticaTaxDetailId = taxDetailId;
            sync.IsTaxLoadedToAcumatica = isTaxLoadedToAcumatica;

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

        public List<UsrShopifyCustomer> RetrieveUnsyncedShopifyCustomers()
        {
            return Entities
                .UsrShopifyCustomers
                .Where(x => !x.UsrShopAcuCustomerSyncs.Any())
                .Where(x => Entities.UsrShopifyOrders.Any(
                    y => y.UsrShopifyCustomer.ShopifyCustomerId == x.ShopifyCustomerId))
                .ToList();
        }

        public List<UsrShopifyCustomer> RetrieveCustomersNeedingUpdate()
        {
            return Entities
                .UsrShopifyCustomers
                .Where(x => x.UsrShopAcuCustomerSyncs.Any() &&
                            x.IsUpdatedInAcumatica == false)
                .ToList();
        }

        public void InsertCustomerSync(UsrShopAcuCustomerSync input)
        {
            Entities.UsrShopAcuCustomerSyncs.Add(input);
            Entities.SaveChanges();
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

        public List<UsrShopifyFulfillment> RetrieveFulfillment(long shopifyOrderId)
        {
            return Entities
                    .UsrShopifyFulfillments
                    .Include(x => x.UsrShopAcuShipmentSyncs)
                    .Where(x => x.ShopifyOrderId == shopifyOrderId)
                    .ToList();
        }

        public bool AnyUnsyncedFulfillments(long shopifyOrderId)
        {
            return Entities
                .UsrShopifyFulfillments
                .Any(x => !x.UsrShopAcuShipmentSyncs.Any());
        }
        

        public void InsertShipmentDetailSync(
                UsrShopifyFulfillment fulfillment, 
                UsrAcumaticaShipmentSalesOrderRef detail)
        {
            var sync = new UsrShopAcuShipmentSync();
            sync.UsrShopifyFulfillment = fulfillment;
            sync.UsrAcumaticaShipmentSalesOrderRef = detail;
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
                            && x.AcumaticaStatus == ShipmentStatus.Open)
                .ToList();
        }

        public List<UsrAcumaticaShipment> 
                        RetrieveShipmentsByMonsterWithNoInvoice()
        {
            return Entities
                .UsrAcumaticaShipments
                .Where(x => x.IsCreatedByMonster 
                            && x.AcumaticaStatus == ShipmentStatus.Confirmed)
                .ToList();
        }


        // Shopify Refunds
        //
        public List<UsrShopifyRefund> RetrieveReturnsNotSynced()
        {
            return RefundRecordGraph
                    .Where(x => x.UsrShopifyOrder.UsrShopAcuOrderSyncs.Any())
                    .Where(x => x.ShopifyIsCancellation == false)
                    .Where(x => x.UsrShopAcuRefundCms.Any() == false ||
                                x.UsrShopAcuRefundCms.Any(y => y.IsComplete == false))
                    .ToList();
        }

        public UsrShopifyRefund RetrieveRefundAndSync(long shopifyRefundId)
        {
            return Entities
                .UsrShopifyRefunds
                .Include(x => x.UsrShopAcuRefundCms)
                .FirstOrDefault(x => x.ShopifyRefundId == shopifyRefundId);
        }

        public List<UsrShopifyRefund> RetrieveCancelsNotSynced()
        {
            return RefundRecordGraph
                .Where(x => x.UsrShopifyOrder.UsrShopAcuOrderSyncs.Any())
                .Where(x => x.ShopifyIsCancellation && !x.IsCancellationSynced)
                .ToList();
        }
        
        private IQueryable<UsrShopifyRefund> RefundRecordGraph => 
            Entities
                .UsrShopifyRefunds
                .Include(x => x.UsrShopifyOrder)
                .Include(x => x.UsrShopifyOrder.UsrShopAcuOrderSyncs)
                .Include(x => x.UsrShopifyOrder.UsrShopAcuOrderSyncs.Select(y => y.UsrAcumaticaSalesOrder));
        
        public void InsertRefundSync(UsrShopAcuRefundCm input)
        {
            Entities.UsrShopAcuRefundCms.Add(input);
            Entities.SaveChanges();
        }


        // Shopify Transactions
        //
        public List<UsrShopifyTransaction> RetrieveUnsyncedPayments()
        {
            return Entities.UsrShopifyTransactions
                .Include(x => x.UsrShopifyOrder)
                .Include(x => x.UsrShopifyOrder.UsrShopifyCustomer)
                .Where(x => x.UsrShopifyOrder.UsrShopAcuOrderSyncs.Any()
                            && x.ShopifyGateway != Gateway.Manual
                            && x.ShopifyStatus == TransactionStatus.Success
                            && (x.ShopifyKind == TransactionKind.Capture
                                || x.ShopifyKind == TransactionKind.Sale
                                || x.ShopifyKind == TransactionKind.Refund)
                            && x.UsrShopifyAcuPayment == null)
                .ToList();
        }


        // NOTE - this does not indicate whether the Credit Memo and Credit Memo Invoice
        // ... have yet been created for this Refund - we'll need to pull the Order Refund
        // ... and its sync record for that
        public List<UsrShopifyTransaction> RetrieveUnsyncedRefunds()
        {
            return Entities.UsrShopifyTransactions
                .Include(x => x.UsrShopifyOrder)
                .Include(x => x.UsrShopifyOrder.UsrShopifyCustomer)
                .Where(x => x.UsrShopifyOrder.UsrShopAcuOrderSyncs.Any()
                            && x.ShopifyGateway != Gateway.Manual
                            && x.ShopifyStatus == TransactionStatus.Success
                            && x.ShopifyKind == TransactionKind.Refund
                            && x.ShopifyRefundId != null
                            && x.UsrShopifyAcuPayment == null)
                .ToList();
        }
        


        public void InsertPayment(UsrShopifyAcuPayment payment)
        {
            Entities.UsrShopifyAcuPayments.Add(payment);
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
