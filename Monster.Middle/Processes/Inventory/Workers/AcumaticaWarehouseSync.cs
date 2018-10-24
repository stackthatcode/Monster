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
    public class AcumaticaWarehouseSync
    {
        private readonly LocationRepository _dataRepository;
        private readonly InventoryApi _shopifyInventoryApi;
        private readonly Acumatica.Api.DistributionClient _acumaticaInventoryApi;
        private readonly IPushLogger _logger;

        public AcumaticaWarehouseSync(
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

        public void BaselineSync()
        {
            var shopifyLocations 
                    = _dataRepository.RetreiveShopifyLocations();

            // TODO - add a Preference for whether or not to push
            // ... Shopify Locations into Acumatica Warehouse

            foreach (var shopifyLocation in shopifyLocations)
            {
                if (shopifyLocation.IsMatched())
                {
                    continue;
                }

                // 
            }
        }

        
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
