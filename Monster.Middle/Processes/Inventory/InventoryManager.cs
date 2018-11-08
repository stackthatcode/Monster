using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Processes.Inventory.Services;
using Monster.Middle.Processes.Inventory.Workers;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Inventory
{
    public class InventoryManager
    {
        private readonly AcumaticaHttpContext _acumaticaContext;
        private readonly AcumaticaWarehousePull _acumaticaWarehousePull;
        private readonly AcumaticaWarehouseSync _acumaticaWarehouseSync;
        private readonly AcumaticaInventoryPull _acumaticaInventoryPull;
        private readonly AcumaticaInventorySync _acumaticaInventorySync;
        
        private readonly ShopifyLocationPull _shopifyLocationPull;
        private readonly ShopifyLocationSync _shopifyLocationSync;
        private readonly ShopifyInventoryPull _shopifyInventoryPull;
        private readonly ShopifyInventorySync _shopifyInventorySync;

        private readonly BatchStateRepository _batchStateRepository;
        private readonly InventoryStatusService _inventoryStatusService;
        private readonly IPushLogger _logger;

        public InventoryManager(
                AcumaticaHttpContext acumaticaContext,
                AcumaticaInventoryPull acumaticaInventoryPull,
                AcumaticaInventorySync acumaticaInventorySync,
                AcumaticaWarehousePull acumaticaWarehousePull,
                AcumaticaWarehouseSync acumaticaWarehouseSync,

                ShopifyLocationPull shopifyLocationPull,
                ShopifyLocationSync shopifyLocationSync,
                ShopifyInventoryPull shopifyInventoryPull,
                ShopifyInventorySync shopifyInventorySync,

                BatchStateRepository batchStateRepository,
                InventoryStatusService inventoryStatusService,
                IPushLogger logger)
        {
            _acumaticaContext = acumaticaContext;

            _acumaticaInventoryPull = acumaticaInventoryPull;
            _acumaticaInventorySync = acumaticaInventorySync;

            _acumaticaWarehousePull = acumaticaWarehousePull;
            _acumaticaWarehouseSync = acumaticaWarehouseSync;

            _shopifyLocationPull = shopifyLocationPull;
            _shopifyLocationSync = shopifyLocationSync;

            _shopifyInventoryPull = shopifyInventoryPull;
            _shopifyInventorySync = shopifyInventorySync;

            _batchStateRepository = batchStateRepository;
            _inventoryStatusService = inventoryStatusService;

            _logger = logger;
        }
        

        public void Reset()
        {
            _batchStateRepository.ResetInventory();
        }
        
        public void SynchronizeLocationOnly()
        {
            // Pull Location from Shopify
            _shopifyLocationPull.Run();

            // Pull Warehouse from Acumatica
            _acumaticaContext.Login();
            _acumaticaWarehousePull.Run();
            _acumaticaContext.Logout();

            // Warehouse-Location Sync
            _acumaticaWarehouseSync.Run();
            _shopifyLocationSync.Run();
        }

        public bool IsLocationStatusValid()
        {
            // Status checkpoint
            var status = _inventoryStatusService.GetCurrentLocationStatus();
            if (!status.OK)
            {
                _logger.Info(status.GetSynopsis());
                return false;
            }
            else
            {
                return true;
            }
        }

        public void SynchronizeInitial()
        {
            SynchronizeLocationOnly();

            if (!IsLocationStatusValid())
            {
                return;
            }

            // Pull Products and Inventory from Shopify
            _shopifyInventoryPull.RunAutomatic();

            // Pull Stock Items and Warehouse Details from Acumatica
            _acumaticaContext.Login();
            _acumaticaInventoryPull.RunAutomatic();
            _acumaticaContext.Logout();
        }

        public void LoadShopifyInventoryIntoAcumatica()
        {
            _acumaticaContext.Login();

            // Synchronize Shopify Inventory to Acumatica
            _acumaticaInventorySync.Run();

            // TODO - control this via a Preference
            _acumaticaInventorySync.RunInventoryReceipts();

            // TODO - control this via a Preference
            _acumaticaInventorySync.RunInventoryReceiptsRelease();

            // Finally, refresh our local cache of Acumatica Inventory
            _acumaticaInventoryPull.RunAutomatic();

            _acumaticaContext.Logout();
        }

        public void SynchronizeRoutine()
        {
            SynchronizeLocationOnly();
            if (!IsLocationStatusValid())
            {
                return;
            }

            _shopifyInventoryPull.RunAutomatic();

            _acumaticaContext.Login();
            _acumaticaInventoryPull.RunAutomatic();
            _acumaticaContext.Logout();

            // Push Acumatica Inventory into Shopify
            _shopifyInventorySync.Run();
        }        
    }
}

