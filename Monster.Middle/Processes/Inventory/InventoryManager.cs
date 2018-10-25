using Monster.Acumatica.Http;
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
        private readonly ShopifyWarehouseSync _shopifyWarehouseSync;
        private readonly ShopifyInventoryPull _shopifyInventoryPull;


        private readonly IPushLogger _logger;

        public InventoryManager(
                AcumaticaHttpContext acumaticaContext,
                AcumaticaInventoryPull acumaticaProductWorker, 
                AcumaticaWarehousePull acumaticaLocationWorker,
                AcumaticaWarehouseSync acumaticaWarehouseSync,
                
                ShopifyInventoryPull shopifyInventoryPull,
                ShopifyLocationPull shopifyLocationPull,
                ShopifyWarehouseSync shopifyWarehouseSync,

                IPushLogger logger)
        {
            _acumaticaContext = acumaticaContext;
            _acumaticaProductWorker = acumaticaProductWorker;
            _acumaticaLocationWorker = acumaticaLocationWorker;
            _acumaticaWarehouseSync = acumaticaWarehouseSync;

            _shopifyInventoryPull = shopifyInventoryPull;
            _shopifyLocationPull = shopifyLocationPull;
            _shopifyWarehouseSync = shopifyWarehouseSync;

            _logger = logger;
        }


        public void BaselineSync()
        {
            // Shopify Pull
            _shopifyLocationPull.BaselinePull();
            //_shopifyProductWorker.BaselinePull();

            // Acumatica Pull
            _acumaticaContext.Begin();
            _acumaticaLocationWorker.BaselinePull();
            //_acumaticaProductWorker.BaselinePull();

            // Warehouse and Location Sync
            _acumaticaWarehouseSync.Synchronize();
            _shopifyWarehouseSync.Synchronize();

        }


        public void DiffSync()
        {

        }
    }
}
