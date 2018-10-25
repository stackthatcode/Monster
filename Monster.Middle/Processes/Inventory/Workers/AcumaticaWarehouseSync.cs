using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Inventory.Workers
{
    public class AcumaticaWarehouseSync
    {
        private readonly LocationRepository _locationRepository;
        private readonly DistributionClient _acumaticaInventoryApi;
        private readonly IPushLogger _logger;

        public AcumaticaWarehouseSync(
                    LocationRepository locationRepository, 
                    DistributionClient acumaticaInventoryApi,
                    IPushLogger logger)
        {
            _locationRepository = locationRepository;
            _acumaticaInventoryApi = acumaticaInventoryApi;
            _logger = logger;
        }

        public void Synchronize()
        {
            var shopifyLocations 
                    = _locationRepository.RetreiveShopifyLocations();

            var warehouses = _locationRepository.RetreiveAcumaticaWarehouses();

            // TODO/ON-HOLD - add a Preference for whether or not to push
            // ... Shopify Locations into Acumatica Warehouse

            foreach (var shopifyLocation in shopifyLocations)
            {
                if (shopifyLocation.UsrAcumaticaWarehouses.Any())
                {
                    var matchedWarehouse 
                        = shopifyLocation.UsrAcumaticaWarehouses.First();

                    // Flag if the names mismatch, now
                    matchedWarehouse.IsNameMismatched =
                        !shopifyLocation.MatchesIdWithName(matchedWarehouse);

                    matchedWarehouse.LastUpdated = DateTime.UtcNow;
                    _locationRepository.SaveChanges();                    
                    continue;
                }

                // Attempt to automatically match
                var automatch = warehouses
                        .FirstOrDefault(x => x.AutoMatches(shopifyLocation));

                if (automatch != null)
                {
                    automatch.ShopifyLocationMonsterId = shopifyLocation.MonsterId;
                    automatch.LastUpdated = DateTime.UtcNow;
                    _locationRepository.SaveChanges();
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

            var newDataRecord = new UsrAcumaticaWarehouse
            {
                ShopifyLocationMonsterId = location.MonsterId,
                AcumaticaWarehouseId
                    = newAcumaticaWarehouse.WarehouseID.value,
                AcumaticaJson = warehouseJson,                
                DateCreated = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
            };

            _locationRepository.InsertAcumaticaWarehouse(newDataRecord);
        }
    }
}
