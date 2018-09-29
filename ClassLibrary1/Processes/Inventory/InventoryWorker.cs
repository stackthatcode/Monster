using Monster.MiddleTier.Persist.Multitenant;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Http.Credentials;

namespace Monster.Middle.Processes.Inventory
{
    public class InventoryWorker
    {
        private readonly InventoryPersistRepository _persistRepository;
        private readonly InventoryApi _inventoryRepository;
        private readonly IPushLogger _logger;

        public void PullFromShopify()
        {
            var locations = _inventoryRepository.RetrieveLocations();
        }
    }
}
