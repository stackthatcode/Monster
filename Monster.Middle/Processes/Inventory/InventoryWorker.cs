using Monster.Middle.Sql.Multitenant;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;

namespace Monster.Middle.Processes.Inventory
{
    public class InventoryWorker
    {
        private readonly InventoryPersistRepository _persistRepository;
        private readonly InventoryApi _inventoryRepository;
        private readonly IPushLogger _logger;

        public InventoryWorker(
                InventoryPersistRepository persistRepository, 
                InventoryApi inventoryRepository, 
                IPushLogger logger)
        {
            _persistRepository = persistRepository;
            _inventoryRepository = inventoryRepository;
            _logger = logger;
        }

        public void PullFromShopify()
        {
            var locations = _inventoryRepository.RetrieveLocations();
        }
    }
}
