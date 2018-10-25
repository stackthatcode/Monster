using Monster.Acumatica.Http;
using Monster.Middle.Processes.Inventory.Services;
using Monster.Middle.Processes.Inventory.Workers;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Inventory
{
    public class InventoryManager
    {
        private readonly AcumaticaHttpContext _acumaticaContext;
        private readonly AcumaticaWarehousePull _acumaticaLocationWorker;
        private readonly AcumaticaInventoryPull _acumaticaProductWorker;
        private readonly AcumaticaWarehouseSync _acumaticaWarehouseSync;

        private readonly ShopifyLocationPull _shopifyLocationPull;
        private readonly ShopifyWarehouseSync _shopifyLocationSync;
        private readonly ShopifyInventoryPull _shopifyInventoryPull;

        private readonly InventoryStatusService _inventoryStatusService;

        private readonly IPushLogger _logger;

        public InventoryManager(
                AcumaticaHttpContext acumaticaContext,
                AcumaticaInventoryPull acumaticaProductWorker, 
                AcumaticaWarehousePull acumaticaLocationWorker,
                AcumaticaWarehouseSync acumaticaWarehouseSync,
                
                ShopifyInventoryPull shopifyInventoryPull,
                ShopifyLocationPull shopifyLocationPull,
                ShopifyWarehouseSync shopifyLocationSync,

                InventoryStatusService inventoryStatusService,

                IPushLogger logger)
        {
            _acumaticaContext = acumaticaContext;
            _acumaticaProductWorker = acumaticaProductWorker;
            _acumaticaLocationWorker = acumaticaLocationWorker;
            _acumaticaWarehouseSync = acumaticaWarehouseSync;

            _shopifyInventoryPull = shopifyInventoryPull;
            _shopifyLocationPull = shopifyLocationPull;
            _shopifyLocationSync = shopifyLocationSync;
            _inventoryStatusService = inventoryStatusService;

            _logger = logger;
        }


        public void RunBaseline()
        {
            _logger.Info("Inventory -> Baseline running...");

            // Warehouse and Location Pull
            _shopifyLocationPull.BaselinePull();
            _acumaticaContext.Begin();
            _acumaticaLocationWorker.BaselinePull();

            // Warehouse and Location Sync
            _acumaticaWarehouseSync.Synchronize();
            _shopifyLocationSync.Synchronize();

            var status = _inventoryStatusService.GetCurrentLocationStatus();
            if (!status.OK)
            {
                _logger.Info("Aborting Inventory -> Baseline:");
                _logger.Info(status.GetSynopsis());
                return;
            }
            
            // Products and Inventory
            _shopifyInventoryPull.BaselinePull();
            _acumaticaProductWorker.BaselinePull();
        }
        
        public void RunDifferential()
        {

        }
    }
}

