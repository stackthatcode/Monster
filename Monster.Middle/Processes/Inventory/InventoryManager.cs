using Monster.Acumatica.Http;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Inventory
{
    public class InventoryManager
    {
        private readonly AcumaticaHttpContext _acumaticaContext;
        private readonly AcumaticaWarehouseWorker _locationWorker;
        private readonly AcumaticaProductWorker _acumaticaProductWorker;

        private readonly ShopifyLocationWorker _shopifyLocationWorker;
        private readonly ShopifyInventoryWorker _shopifyProductWorker;
        private readonly IPushLogger _logger;

        public InventoryManager(
                AcumaticaHttpContext acumaticaContext,
                ShopifyInventoryWorker shopifyProductWorker, 
                AcumaticaProductWorker acumaticaProductWorker, 
                AcumaticaWarehouseWorker locationWorker, IPushLogger logger, ShopifyLocationWorker shopifyLocationWorker)
        {
            _acumaticaContext = acumaticaContext;
            _shopifyProductWorker = shopifyProductWorker;
            _acumaticaProductWorker = acumaticaProductWorker;
            _locationWorker = locationWorker;
            _logger = logger;
            _shopifyLocationWorker = shopifyLocationWorker;
        }

        public void BaselineSync()
        {
            // Shopify Pull
            _shopifyLocationWorker.BaselinePullLocations();
            _shopifyProductWorker.BaselinePullProducts();

            // Acumatica Pull
            _acumaticaContext.Begin();
            _locationWorker.BaselinePullWarehouses();
            _acumaticaProductWorker.BaselinePullStockItems();
        }

        public void DiffSync()
        {

        }
    }
}
