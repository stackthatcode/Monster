using Push.Foundation.Web.HttpClient;
using Push.Shopify.Api.Shop;
using Push.Shopify.Config;
using Push.Shopify.HttpClient;

namespace Push.Shopify.Api
{
    public class ShopApiRepository
    {
        private readonly ShopifyRequestBuilder _requestFactory;
        private readonly ClientFacade _executionFacade;
        
        public ShopApiRepository(
                ShopifyRequestBuilder requestFactory,
                ClientFacade executionFacade,
                ShopifyClientSettings settings)
        {
            _executionFacade = executionFacade;
            _executionFacade.Settings = settings;
            _requestFactory = requestFactory;
        }

        public virtual ShopDto Retrieve()
        {
            var path = "/admin/shop.json";                       
            var request = _requestFactory.HttpGet(path);
            var clientResponse = _executionFacade.ExecuteRequest(request);

            var output = ShopDto.MakeFromJson(clientResponse.Body);
            return output;
        }        
    }
}

