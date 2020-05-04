using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Push.Foundation.Utilities.Json;


namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaWarehouseGet
    {
        private readonly AcumaticaInventoryRepository _dataRepository;
        private readonly DistributionClient _acumaticaInventoryApi;
        private readonly AcumaticaJsonService _acumaticaJsonService;


        public AcumaticaWarehouseGet(
                AcumaticaInventoryRepository dataRepository,
                DistributionClient acumaticaInventoryApi,
                AcumaticaJsonService acumaticaJsonService)
        {
            _dataRepository = dataRepository;
            _acumaticaInventoryApi = acumaticaInventoryApi;
            _acumaticaJsonService = acumaticaJsonService;
        }

        public void Run()
        {
            var warehouses =
                _acumaticaInventoryApi.RetrieveWarehouses().DeserializeFromJson<List<Warehouse>>();
            
            var warehouseRecords = _dataRepository.RetrieveWarehouses();

            foreach (var warehouse in warehouses)
            {
                using (var transaction = _dataRepository.BeginTransaction())
                {
                    var warehouseRecord = warehouseRecords.FindByAcumaticaId(warehouse);

                    if (warehouseRecord == null)
                    {
                        var newDataWarehouse = new AcumaticaWarehouse
                        {
                            AcumaticaWarehouseId = warehouse.WarehouseID.value,
                            DateCreated = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow,
                        };

                        _dataRepository.InsertWarehouse(newDataWarehouse);
                    }
                    else
                    {
                        warehouseRecord.LastUpdated = DateTime.UtcNow;
                        _dataRepository.SaveChanges();
                    }

                    _acumaticaJsonService.Upsert(
                        AcumaticaJsonType.SalesOrderShipments, warehouse.WarehouseID.value, warehouse.SerializeToJson());
                    transaction.Commit();
                }
            }
        }
    }
}

