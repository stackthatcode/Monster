using Monster.Middle.EF;
using Monster.Middle.EF.Inventory;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Http.Credentials;

namespace Monster.Middle.Processes.Inventory
{
    public class InventoryWorker
    {
        private readonly InventoryPersistRepository _persistRepository;
        private readonly ShopifyApiFactory _shopifyApiFactory;
        private readonly IPushLogger _logger;

        public void PullFromShopify(IShopifyCredentials credentials)
        {
            var shopifyRepository = _shopifyApiFactory.MakeInventoryApi(credentials);

            var locations = shopifyRepository.RetrieveLocations();
        }
    }
}
