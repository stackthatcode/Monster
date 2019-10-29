using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Misc.State;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Orders;

namespace Monster.Middle.Processes.Sync.Persist
{
    public class AnalyzerRepository
    {
        private readonly ProcessPersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;


        public AnalyzerRepository(ProcessPersistContext dataContext)
        {
            _dataContext = dataContext;
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


    }
}
