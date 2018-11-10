using System;
using System.Linq;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Shopify;
using Monster.Middle.Persist.Multitenant.Sync;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;


namespace Monster.Middle.Processes.Inventory.Workers
{
    public class ShopifyInventorySync
    {
        private readonly InventoryApi _inventoryApi;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly ShopifyInventoryRepository _inventoryRepository;
        private readonly AcumaticaInventoryRepository _acumaticaInventoryRepository;

        // Possibly expand - this is a one-time thing...
        public const int InitialBatchStateFudgeMin = -15;


        public ShopifyInventorySync(
                InventoryApi inventoryApi,
                ShopifyInventoryRepository inventoryRepository, 
                AcumaticaInventoryRepository acumaticaInventoryRepository, 
                SyncInventoryRepository syncInventoryRepository)
        {
            _inventoryApi = inventoryApi;
            _inventoryRepository = inventoryRepository;
            _acumaticaInventoryRepository = acumaticaInventoryRepository;
            _syncInventoryRepository = syncInventoryRepository;
        }


        public void Run()
        {
            var warehouseDetails
                = _syncInventoryRepository
                    .RetrieveWarehouseDetailsNotSynced();

            var warehouses = _syncInventoryRepository.RetrieveWarehouses();

            foreach (var warehouseDetail in warehouseDetails)
            {
                var quantity = (int)warehouseDetail.AcumaticaQtyOnHand;

                // Extracts the Shopify Location
                // NOTE - requires valid Warehouse-Location matching
                var warehouse = warehouses.ByDetail(warehouseDetail);                    
                var location = warehouse.MatchedLocation();

                var locationMonsterId = location.MonsterId;
                var shopifyLocationId = location.ShopifyLocationId;

                // Extracts the Matched Variant
                var stockItem
                    = _syncInventoryRepository.RetrieveStockItem(
                        warehouseDetail.UsrAcumaticaStockItem.ItemId);

                var shopifyInventoryItemId =
                    stockItem.MatchedVariant().ShopifyInventoryItemId;

                var inventoryLevels =
                    stockItem.MatchedVariant().UsrShopifyInventoryLevels;

                var inventoryLevel =
                    inventoryLevels.First(
                        x => x.LocationMonsterId == locationMonsterId);

                var level = new InventoryLevel
                {
                    inventory_item_id = shopifyInventoryItemId,
                    available = quantity,
                    location_id = shopifyLocationId,
                };

                var levelJson = level.SerializeToJson();
                var resultJson = _inventoryApi.SetInventoryLevels(levelJson);

                // Flag Acumatica Warehouse Detail as synchronized
                warehouseDetail.IsShopifySynced = true;

                // Update Shopify Inventory Level records
                inventoryLevel.ShopifyAvailableQuantity = quantity;
                inventoryLevel.LastUpdated = DateTime.UtcNow;

                _inventoryRepository.SaveChanges();
            }
        }
    }
}
