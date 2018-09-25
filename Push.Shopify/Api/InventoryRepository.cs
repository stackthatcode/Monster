using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Http;

namespace Push.Shopify.Api
{
    public class InventoryRepository
    {
        private readonly HttpFacade _executionFacade;
        private readonly IPushLogger _logger;

        public InventoryRepository(HttpFacade executionFacade, IPushLogger logger)
        {
            _executionFacade = executionFacade;
            _logger = logger;
        }

        public string RetrieveLocations()
        {
            var path = $"/admin/locations.json";
            var response = _executionFacade.Get(path);
            return response.Body;
        }
    }
}
