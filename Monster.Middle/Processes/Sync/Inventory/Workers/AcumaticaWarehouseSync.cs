using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Sync.Inventory.Workers
{
    public class AcumaticaWarehouseSync
    {
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly AcumaticaInventoryRepository _acumaticaInventoryRepository;
        private readonly DistributionClient _acumaticaInventoryApi;
        private readonly IPushLogger _logger;

        public AcumaticaWarehouseSync(
                    SyncInventoryRepository syncInventoryRepository,
                    DistributionClient acumaticaInventoryApi,
                    IPushLogger logger)
        {
            _syncInventoryRepository = syncInventoryRepository;
            _acumaticaInventoryApi = acumaticaInventoryApi;
            _logger = logger;
        }

        public void Run()
        {
            var shopifyLocations 
                    = _syncInventoryRepository.RetrieveLocations();

            var warehouses 
                    = _syncInventoryRepository.RetrieveWarehouses();
                

            // TODO/ON-HOLD - add a Preference for whether or not to push
            // ... Shopify Locations into Acumatica Warehouse

            foreach (var shopifyLocation in shopifyLocations)
            {
                if (shopifyLocation.UsrShopAcuWarehouseSyncs.Any())
                {
                    // TODO - resolve this
                    //var sync
                    //    = shopifyLocation
                    //        .UsrShopAcuWarehouseSyncs
                    //        .First();
                    // Flag if the names mismatch, now
                    //sync.IsNameMismatched =
                    //    !shopifyLocation.MatchesIdWithName(matchedWarehouse);

                    if (shopifyLocation.IsImproperlyMatched())
                    {
                        _syncInventoryRepository.DeleteWarehouseSync(
                                shopifyLocation
                                    .UsrShopAcuWarehouseSyncs
                                    .First());
                    }

                    continue;
                }

                // Attempt to automatically match
                var automatchedWarehouse = warehouses
                        .FirstOrDefault(x => x.AutoMatches(shopifyLocation));

                if (automatchedWarehouse != null)
                {

                    _syncInventoryRepository
                        .InsertWarehouseSync(shopifyLocation, automatchedWarehouse);
                    continue;
                }

                // DISABLED - pending Acumatica API updates...?
                //PushWarehouseToAcumatica(shopifyLocation);
            }
        }

        public void PushWarehouseToAcumatica(UsrShopifyLocation location)
        {
            var standardizedName = location.StandardizedName();
            
            var newAcuWarehouse = new Warehouse
            {
                Active = true.ToValue(),
                WarehouseID = standardizedName.ToValue(),

                // TODO - neither of these appear to work
                Country = "US".ToValue(),
                Address = new WarehouseAddress
                {
                    Country = "US".ToValue()
                },

                Locations = new List<WarehouseLocation>
                {
                    new WarehouseLocation
                    {
                        LocationID = "Default".ToValue(),
                        Active = true.ToValue(),
                        
                    }
                }
            };

            var warehouseJson =
                _acumaticaInventoryApi.AddNewWarehouse(
                    newAcuWarehouse.SerializeToJson());

            var newAcumaticaWarehouse 
                    = warehouseJson.DeserializeFromJson<Warehouse>();

            var newWarehouseRecord = new UsrAcumaticaWarehouse
            {
                AcumaticaWarehouseId
                    = newAcumaticaWarehouse.WarehouseID.value,
                AcumaticaJson = warehouseJson,                
                DateCreated = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
            };

            _acumaticaInventoryRepository.InsertWarehouse(newWarehouseRecord);

            _syncInventoryRepository.InsertWarehouseSync(location, newWarehouseRecord);
        }
    }
}
