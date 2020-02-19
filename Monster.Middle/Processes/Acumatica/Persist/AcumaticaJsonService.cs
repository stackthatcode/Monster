using System;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Distribution;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Json;


namespace Monster.Middle.Processes.Acumatica.Persist
{
    public class AcumaticaJsonService
    {
        private readonly ProcessPersistContext _dataContext;
        private readonly DistributionClient _distributionClient;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly CustomerClient _customerClient;
        
        public MonsterDataContext Entities => _dataContext.Entities;

        public AcumaticaJsonService(
                ProcessPersistContext dataContext, 
                CustomerClient customerClient, 
                DistributionClient distributionClient, 
                SalesOrderClient salesOrderClient)
        {
            _dataContext = dataContext;
            _customerClient = customerClient;
            _distributionClient = distributionClient;
            _salesOrderClient = salesOrderClient;
        }

        public void Upsert(int acumaticaJsonType, string acumaticaNbr, string json)
        {
            Upsert(acumaticaJsonType, acumaticaNbr, String.Empty, json);
        }

        public void Upsert(int acumaticaJsonType, string acumaticaNbr, string acumaticaType, string json)
        {
            var record = RetrieveRecordOnly(acumaticaJsonType, acumaticaNbr, acumaticaType);
            record.Json = json;
            Entities.SaveChanges();
        }

        public StockItem RetrieveStockItem(string itemId, bool rehydrate = true)
        {
            return  RetrieveJson(AcumaticaJsonType.StockItem, itemId, rehydrate).DeserializeFromJson<StockItem>();
        }

        public SalesOrder RetrieveSalesOrder(string orderNbr)
        {
            return RetrieveJson(AcumaticaJsonType.SalesOrderShipments, orderNbr, SalesOrderType.SO)
                    .DeserializeFromJson<SalesOrder>();
        }


        public string RetrieveJson(int acumaticaJsonType, string acumaticaNbr, bool rehydrate = true)
        {
            return RetrieveJson(acumaticaJsonType, acumaticaNbr, String.Empty, rehydrate);
        }

        public string RetrieveJson(
                    int acumaticaJsonType, string acumaticaNbr, string acumaticaType = "", bool rehydrate = true)
        {
            var record = RetrieveRecordOnly(acumaticaJsonType, acumaticaNbr, acumaticaType);

            if (record.Json != null)
            {
                return record.Json;
            }
            else
            {
                var json = RetrieveExternalJson(acumaticaJsonType, acumaticaNbr, acumaticaType);
                record.Json = json;
                record.LastAccessed = DateTime.UtcNow;
                Entities.SaveChanges();
                return json;
            }
        }

        public AcumaticaJsonStore RetrieveRecordOnly(
                int acumaticaJsonType, string acumaticaNbr, string acumaticaType = null)
        {
            var output = Entities
                .AcumaticaJsonStores
                .FirstOrDefault(x => x.AcumaticaJsonType == acumaticaJsonType 
                                     && x.AcumaticaNbr == acumaticaNbr
                                     && x.AcumaticaType == acumaticaType);

            if (output == null)
            {
                var newRecord = new AcumaticaJsonStore();
                newRecord.AcumaticaJsonType = acumaticaJsonType;
                newRecord.AcumaticaNbr = acumaticaNbr;
                newRecord.AcumaticaType = acumaticaType;
                newRecord.LastAccessed = DateTime.UtcNow;
                Entities.AcumaticaJsonStores.Add(newRecord);
                Entities.SaveChanges();
                return newRecord;
            }
            else
            {
                output.LastAccessed = DateTime.UtcNow;
                Entities.SaveChanges();
                return output;
            }
        }

        private string RetrieveExternalJson(int acumaticaJsonType, string acumaticaNbr, string acumaticaType)
        {
            if (acumaticaJsonType == AcumaticaJsonType.Warehouse)
            {
                return _distributionClient.RetrieveWarehouse(acumaticaNbr);
            }
            if (acumaticaJsonType == AcumaticaJsonType.Customer)
            {
                return _customerClient.RetrieveCustomer(acumaticaNbr);
            }
            if (acumaticaJsonType == AcumaticaJsonType.StockItem)
            {
                return _distributionClient.RetrieveStockItem(acumaticaNbr);
            }
            if (acumaticaJsonType == AcumaticaJsonType.SalesOrderShipments)
            {
                return _salesOrderClient.RetrieveSalesOrder(acumaticaType, acumaticaNbr);
            }

            throw new NotImplementedException();
        }
    }
}
