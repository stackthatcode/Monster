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

        private readonly ShopifyLocationWorker _shopifyLocationWorker;
        private readonly ShopifyInventoryPullWorker _shopifyProductWorker;


        private readonly IPushLogger _logger;

        public InventoryManager(
                AcumaticaHttpContext acumaticaContext,
                ShopifyInventoryPullWorker shopifyProductWorker, 
                AcumaticaInventoryPull acumaticaProductWorker, 
                AcumaticaWarehousePull acumaticaLocationWorker, 
                ShopifyLocationWorker shopifyLocationWorker,
                IPushLogger logger)
        {
            _acumaticaContext = acumaticaContext;
            _shopifyProductWorker = shopifyProductWorker;
            _acumaticaProductWorker = acumaticaProductWorker;
            _acumaticaLocationWorker = acumaticaLocationWorker;
            _logger = logger;
            _shopifyLocationWorker = shopifyLocationWorker;
        }

        public void BaselineSync()
        {
            // Shopify Pull
            //_shopifyLocationWorker.BaselinePull();
            //_shopifyProductWorker.BaselinePull();

            // Acumatica Pull
            _acumaticaContext.Begin();
            _acumaticaLocationWorker.BaselinePull();
            //_acumaticaProductWorker.BaselinePull();

            // Acumatica Sync
            // TODO - Sync Warehouses
            
        }

        public void DiffSync()
        {

        }
    }
}
