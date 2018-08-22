using System;
using Push.Foundation.Web.HttpClient;
using Push.Shopify.Config;
using Push.Shopify.HttpClient;

namespace Push.Shopify.Api
{
    public class PayoutApiRepository
    {
        private readonly ShopifyRequestBuilder _requestFactory;
        private readonly HttpFacade _executionFacade;
        
        public PayoutApiRepository(
                ShopifyRequestBuilder requestFactory,
                HttpFacade executionFacade,
                ShopifyClientSettings settings)
        {
            _executionFacade = executionFacade;
            _executionFacade.Settings = settings;
            _requestFactory = requestFactory;
        }

        public virtual string RetrievePayouts()
        {
            var path = "/admin/shopify_payments/payouts.json";
            var request = _requestFactory.HttpGet(path);
            var clientResponse = _executionFacade.ExecuteRequest(request);

            var output = clientResponse.Body;
            return output;
        }

        public virtual string RetrievePayoutDetail(long id)
        {            
            var path = $"/admin/shopify_payments/balance/transactions.json?payout_id={id}";
            var request = _requestFactory.HttpGet(path);
            var clientResponse = _executionFacade.ExecuteRequest(request);

            var output = clientResponse.Body;
            return output;
        }        
    }
}

