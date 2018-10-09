using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;

namespace Monster.Middle.Processes.Inventory
{
    public class ProductWorker
    {
        private readonly InventoryRepository _dataRepository;
        private readonly ProductApi _shopifyProductApi;
        private readonly Acumatica.Api.InventoryRepository _acumaticaProductApi;
        private readonly IPushLogger _logger;

        public ProductWorker(
                InventoryRepository dataRepository, 
                Acumatica.Api.InventoryRepository acumaticaInventoryApi, 
                ProductApi shopifyProductApi)
        {
            _dataRepository = dataRepository;
            _shopifyProductApi = shopifyProductApi;
        }


        public void PullProductsFromShopify()
        {
        }
    }
}
