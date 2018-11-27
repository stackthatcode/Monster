using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Extensions;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaWarehousePull
    {
        private readonly AcumaticaInventoryRepository _dataRepository;
        private readonly DistributionClient _acumaticaInventoryApi;
        private readonly IPushLogger _logger;

        public AcumaticaWarehousePull(
                AcumaticaInventoryRepository dataRepository,
                DistributionClient acumaticaInventoryApi,
                IPushLogger logger)
        {
            _dataRepository = dataRepository;
            _acumaticaInventoryApi = acumaticaInventoryApi;
            _logger = logger;
        }

        public void Run()
        {
            var warehouses =
                _acumaticaInventoryApi
                    .RetrieveWarehouses()
                    .DeserializeFromJson<List<Warehouse>>();
            
            var warehouseRecords = _dataRepository.RetreiveWarehouses();

            foreach (var warehouse in warehouses)
            {
                var dataWarehouse = warehouseRecords.FindByAcumaticaId(warehouse);

                if (dataWarehouse == null)
                {
                    var newDataWarehouse = new UsrAcumaticaWarehouse
                    {
                        AcumaticaWarehouseId = warehouse.WarehouseID.value,
                        AcumaticaJson = warehouse.SerializeToJson(),
                        DateCreated = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                    };

                    _dataRepository.InsertAcumaticaWarehouse(newDataWarehouse);
                }
                else
                {
                    dataWarehouse.AcumaticaJson = warehouse.SerializeToJson();
                    dataWarehouse.LastUpdated = DateTime.UtcNow;
                    _dataRepository.SaveChanges();
                }
            }
        }
        
        
    }
}

