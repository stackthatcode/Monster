using System;
using System.Linq;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Shopify;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;


namespace Monster.Middle.Processes.Inventory.Workers
{
    public class ShopifyInventorySync
    {
        private readonly ProductApi _productApi;
        private readonly InventoryApi _inventoryApi;
        private readonly ShopifyInventoryRepository _inventoryRepository;
        private readonly AcumaticaInventoryRepository _acumaticaInventoryRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;

        // Possibly expand - this is a one-time thing...
        public const int InitialBatchStateFudgeMin = -15;


        public ShopifyInventorySync(
                IPushLogger logger,
                ProductApi productApi,
                InventoryApi inventoryApi,
                ShopifyInventoryRepository inventoryRepository, 
                AcumaticaInventoryRepository acumaticaInventoryRepository,
                BatchStateRepository batchStateRepository)
        {
            _productApi = productApi;
            _inventoryApi = inventoryApi;
            _inventoryRepository = inventoryRepository;
            _acumaticaInventoryRepository = acumaticaInventoryRepository;
            _batchStateRepository = batchStateRepository;
            _logger = logger;
        }


        public void Run()
        {
            var warehouseDetails
                = _acumaticaInventoryRepository
                    .RetrieveWarehouseDetailsNotSynced();

            foreach (var warehouseDetail in warehouseDetails)
            {
                var quantity = (int)warehouseDetail.AcumaticaQtyOnHand;

                var location =
                    warehouseDetail
                        .UsrAcumaticaWarehouse
                        .UsrShopifyLocation;

                var shopifyLocationId = location.ShopifyLocationId;
                var locationMonsterId = location.MonsterId;

                var shopifyInventoryItemId =
                    warehouseDetail
                        .UsrAcumaticaStockItem
                        .UsrShopifyVariant
                        .ShopifyInventoryItemId;

                var inventoryLevelData
                    = warehouseDetail
                        .UsrAcumaticaStockItem
                        .UsrShopifyVariant
                        .UsrShopifyInventoryLevels
                        .First(x => x.LocationMonsterId == locationMonsterId);

                var level = new InventoryLevel
                {
                    inventory_item_id = shopifyInventoryItemId,
                    available = quantity,
                    location_id = shopifyLocationId,
                };

                var levelJson = level.SerializeToJson();
                var resultJson = _inventoryApi.SetInventoryLevels(levelJson);

                // Flag Acumatica Warehouse Detail as synchronized
                warehouseDetail.ShopifyIsSynced = true;

                // Update Shopify Inventory Level records
                inventoryLevelData.ShopifyAvailableQuantity = quantity;
                inventoryLevelData.LastUpdated = DateTime.UtcNow;

                _inventoryRepository.SaveChanges();
            }
        }
    }
}
