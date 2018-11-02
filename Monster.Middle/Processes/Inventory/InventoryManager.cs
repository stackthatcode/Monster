using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant;
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


        public void RunBaseline()
        {
            _logger.Info("Inventory -> Baseline running...");

            _batchStateRepository.ResetInventoryBatchState();
            _acumaticaContext.Begin();
            RunLocationSync();

            if (!IsInventoryStatusOk())
            {
                return;
            }

            // Products and Inventory Pull
            _shopifyInventoryPull.RunAll();
            _acumaticaInventoryPull.RunAll();
            
            // This is a one-time operation that will create Warehouse Receipts
            // TODO - control this via a Preference
            RunShopifyToAcumaticaInventorySync();

            // Push Acumatica Inventory into Shopify
            _shopifyInventorySync.Run();
        }
        
        public void RunUpdate()
        {
            RunLocationSync();

            if (!IsInventoryStatusOk())
            {
                return;
            }

            _shopifyInventoryPull.RunUpdated();
            _acumaticaInventoryPull.RunUpdated();

            // TODO - control this via a Preference
            RunShopifyToAcumaticaInventorySync();

            _shopifyInventorySync.Run();
        }

        public void RunLocationSync()
        {
            // Warehouse and Location Pull
            _shopifyLocationPull.Run();
            _acumaticaWarehousePull.Run();

            // Warehouse and Location Sync
            _acumaticaWarehouseSync.Run();
            _shopifyLocationSync.Run();
        }

        public void RunShopifyToAcumaticaInventorySync()
        {
            // Synchronize Shopify Inventory to Acumatica
            _acumaticaInventorySync.Run();
            
            // TODO - control this via a Preference
            _acumaticaInventorySync.RunInventoryReceipts();

            // TODO - control this via a Preference
            _acumaticaInventorySync.RunInventoryReceiptsRelease();

            _acumaticaInventoryPull.RunUpdated();
        }

        public bool IsInventoryStatusOk()
        {
            // Status checkpoint
            var status = _inventoryStatusService.GetCurrentLocationStatus();
            if (!status.OK)
            {
                _logger.Info("Aborting Inventory -> Baseline:");
                _logger.Info(status.GetSynopsis());
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}

