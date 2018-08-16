using System;
using Push.Foundation.Web.HttpClient;
using Push.Shopify.Config;
using Push.Shopify.HttpClient;

namespace Push.Shopify.Api
{
    public class PayoutApiRepository
    {
        private readonly ShopifyRequestBuilder _requestFactory;
        private readonly ClientFacade _executionFacade;
        
        public PayoutApiRepository(
                ShopifyRequestBuilder requestFactory,
                ClientFacade executionFacade,
                ShopifyClientSettings settings)
        {
            _executionFacade = executionFacade;
            _executionFacade.Settings = settings;
            _requestFactory = requestFactory;
        }

        public virtual string RetrievePayouts(DateTimeOffset date)
        {
            var formattedDate = date.ToString("yyyy-MM-dd");

            //var path = "/admin/shopify_payments/payouts.json";

            // Gets the payouts
            //var path = $"/admin/payments/transactions.json?payout_date={formattedDate}";                       

            var path = "/admin/shopify_payments/balance/transactions.json?payout_id=18795823204";
            var request = _requestFactory.HttpGet(path);
            var clientResponse = _executionFacade.ExecuteRequest(request);

            var output = clientResponse.Body;
            return output;
        }        
    }
}

