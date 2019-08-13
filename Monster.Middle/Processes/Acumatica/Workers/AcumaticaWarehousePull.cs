using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Model.Extensions;
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
            
            var warehouseRecords = _dataRepository.RetrieveWarehouses();

            foreach (var warehouse in warehouses)
            {
                var warehouseRecord = warehouseRecords.FindByAcumaticaId(warehouse);

                if (warehouseRecord == null)
                {
                    var newDataWarehouse = new UsrAcumaticaWarehouse
                    {
                        AcumaticaWarehouseId = warehouse.WarehouseID.value,
                        AcumaticaJson = warehouse.SerializeToJson(),
                        DateCreated = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                    };

                    _dataRepository.InsertWarehouse(newDataWarehouse);
                }
                else
                {
                    warehouseRecord.AcumaticaJson = warehouse.SerializeToJson();
                    warehouseRecord.LastUpdated = DateTime.UtcNow;
                    _dataRepository.SaveChanges();
                }
            }
        }
    }
}

