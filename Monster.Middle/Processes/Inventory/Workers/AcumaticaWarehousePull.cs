using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;

namespace Monster.Middle.Processes.Inventory.Workers
{
    public class AcumaticaWarehousePull
    {
        private readonly LocationRepository _dataRepository;
        private readonly InventoryApi _shopifyInventoryApi;
        private readonly Acumatica.Api.DistributionClient _acumaticaInventoryApi;
        private readonly IPushLogger _logger;

        public AcumaticaWarehousePull(
                IPushLogger logger,
                LocationRepository dataRepository, 
                InventoryApi shopifyInventoryApi, 
                Acumatica.Api.DistributionClient acumaticaInventoryApi)
        {
            _dataRepository = dataRepository;
            _shopifyInventoryApi = shopifyInventoryApi;
            _acumaticaInventoryApi = acumaticaInventoryApi;
            _logger = logger;
        }

        public void Pull()
        {
            var warehouses =
                _acumaticaInventoryApi
                    .RetrieveWarehouses()
                    .DeserializeFromJson<List<Warehouse>>();
            
            var dataWarehouses = 
                _dataRepository.RetreiveAcumaticaWarehouses();

            foreach (var warehouse in warehouses)
            {
                var dataWarehouse = dataWarehouses.FindByAcumaticaId(warehouse);

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

