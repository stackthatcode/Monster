using Push.Foundation.Web.HttpClient;
using Push.Shopify.Config;
using Push.Shopify.HttpClient;

namespace Push.Shopify.Api
{
    public class ShopApiRepository
    {
        private readonly ShopifyRequestBuilder _requestFactory;
        private readonly HttpFacade _executionFacade;
        
        public ShopApiRepository(
                ShopifyRequestBuilder requestFactory,
                HttpFacade executionFacade,
                ShopifyClientSettings settings)
        {
            _executionFacade = executionFacade;
            _executionFacade.Settings = settings;
            _requestFactory = requestFactory;
        }

        public virtual string Retrieve()
        {
            var path = "/admin/shop.json";                       
            var request = _requestFactory.HttpGet(path);
            var clientResponse = _executionFacade.ExecuteRequest(request);

            var output = clientResponse.Body;
            return output;
        }        
    }
}

