using Push.Foundation.Web.Http;

namespace Push.Shopify.Api
{
    public class ShopRepository
    {
        private readonly HttpFacade _executionFacade;
        
        public ShopRepository(HttpFacade executionFacade)
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

