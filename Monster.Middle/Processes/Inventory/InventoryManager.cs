using Monster.Acumatica.Http;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Inventory
{
    public class InventoryManager
    {
        private readonly AcumaticaHttpContext _acumaticaContext;
        private readonly AcumaticaWarehouseWorker _acumaticaLocationWorker;
        private readonly AcumaticaInventoryPullWorker _acumaticaProductWorker;

        private readonly ShopifyLocationWorker _shopifyLocationWorker;
        private readonly ShopifyInventoryPullWorker _shopifyProductWorker;
        private readonly IPushLogger _logger;

        public InventoryManager(
                AcumaticaHttpContext acumaticaContext,
                ShopifyInventoryPullWorker shopifyProductWorker, 
                AcumaticaInventoryPullWorker acumaticaProductWorker, 
                AcumaticaWarehouseWorker acumaticaLocationWorker, 
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
            _shopifyLocationWorker.BaselinePull();
            _shopifyProductWorker.BaselinePull();

            // Acumatica Pull
            _acumaticaContext.Begin();
            _acumaticaLocationWorker.BaselinePull();
            _acumaticaProductWorker.BaselinePull();

            // Acumatica Sync
            // TODO - Acumatica Sync Worker
        }

        public void DiffSync()
        {

        }
    }
}
