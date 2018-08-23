using Push.Foundation.Web.HttpClient;
using Push.Shopify.Config;
using Push.Shopify.HttpClient;

namespace Push.Shopify.Api
{
    public class ShopApiRepository
    {
        private readonly HttpFacade _executionFacade;
        
        public ShopApiRepository(HttpFacade executionFacade)
        {
            _executionFacade = executionFacade;
        }

        public virtual string Retrieve()
        {
            var path = "/admin/shop.json";                       
            var clientResponse = _executionFacade.Get(path);

            var output = clientResponse.Body;
            return output;
        }        
    }
}

