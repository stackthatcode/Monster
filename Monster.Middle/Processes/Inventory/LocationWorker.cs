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
using Push.Shopify.Api.Inventory;
using InventoryRepository = Monster.Middle.Persist.Multitenant.InventoryRepository;

namespace Monster.Middle.Processes.Inventory
{
    public class LocationWorker
    {
        private readonly InventoryRepository _dataRepository;
        private readonly InventoryApi _shopifyInventoryApi;
        private readonly Acumatica.Api.InventoryClient _acumaticaInventoryApi;
        private readonly IPushLogger _logger;

        public LocationWorker(
                IPushLogger logger,
                InventoryRepository dataRepository, 
                InventoryApi shopifyInventoryApi, 
                Acumatica.Api.InventoryClient acumaticaInventoryApi)
        {
            _dataRepository = dataRepository;
            _shopifyInventoryApi = shopifyInventoryApi;
            _acumaticaInventoryApi = acumaticaInventoryApi;
            _logger = logger;
        }

        public void BaselinePullWarehouses()
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
                    _dataRepository.SaveChanges();
                }
            }
        }

        public void FlagDifferencesAndExceptions()
        {
            var acumaticaWarehouses 
                    = _dataRepository.RetreiveAcumaticaWarehouses();

            var shopifyLocations 
                    = _dataRepository.RetreiveShopifyLocations();

            foreach (var location in shopifyLocations)
            {
                if (!acumaticaWarehouses.Any(x => x.Matches(location)))
                {
                    var message =
                        $"Shopify Location {location.ShopifyLocationName} " +
                        $"does not have a matching Acumatica Warehouse";
                    _logger.Info(message);
                }
            }

            foreach (var warehouse in acumaticaWarehouses)
            {
                if (!shopifyLocations.Any(x => x.Matches(warehouse)))
                {
                    var message =
                        $"Acumatica Warehouse {warehouse.AcumaticaWarehouseId} " +
                        $"does not have a matching Shopify Location";
                    _logger.Info(message);
                }
            }
        }


        // This is on-hold pending API fixes
        public void CreateWarehouseInAcumatica(UsrShopifyLocation location)
        {
            var newAcuWarehouse = new Warehouse
            {
                Active = true.ToValue(),
                WarehouseID = location.ShopifyLocationName.ToValue(),
            };

            var warehouseJson =
                _acumaticaInventoryApi.AddNewWarehouse(
                    newAcuWarehouse.SerializeToJson());

            var newAcumaticaWarehouse = warehouseJson.DeserializeFromJson<Warehouse>();

            var newDataRecord = new UsrAcumaticaWarehouse
            {
                AcumaticaWarehouseId
                    = newAcumaticaWarehouse.WarehouseID.value,
                AcumaticaJson = warehouseJson,
                DateCreated = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
            };

            _dataRepository.InsertAcumaticaWarehouse(newDataRecord);
        }
    }
}
