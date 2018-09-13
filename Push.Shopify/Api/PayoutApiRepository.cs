using Push.Foundation.Web.HttpClient;

namespace Push.Shopify.Api
{
    public class PayoutApiRepository
    {
        private readonly HttpFacade _executionFacade;
        
        public PayoutApiRepository(HttpFacade executionFacade)
        {
            _executionFacade = executionFacade;
        }

        public virtual string RetrievePayouts()
        {
            var path = "/admin/shopify_payments/payouts.json";
            var clientResponse = _executionFacade.Get(path);

            var output = clientResponse.Body;
            return output;
        }

        public virtual string RetrievePayoutDetail(long id)
        {            
            var path = $"/admin/shopify_payments/balance/transactions.json?payout_id={id}";
            var clientResponse = _executionFacade.Get(path);

            var output = clientResponse.Body;
            return output;
        }        
    }
}

