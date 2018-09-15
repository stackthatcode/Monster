using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Http;
using Push.Foundation.Web.Misc;

namespace Push.Shopify.Api
{
    public class PayoutRepository
    {
        private readonly HttpFacade _executionFacade;
        
        public PayoutRepository(HttpFacade executionFacade)
        {
            _executionFacade = executionFacade;
        }

        public virtual string RetrievePayouts(int limit = 50, int page = 1)
        {
            var queryString 
                = new QueryStringBuilder()
                    .Add("limit", limit)
                    .Add("page", page)
                    .ToString();

            var path = $"/admin/shopify_payments/payouts.json?{queryString}";
            var clientResponse = _executionFacade.Get(path);

            var output = clientResponse.Body;
            return output;
        }

        public virtual string RetrievePayoutDetail(
                long? payout_id = null, long? since_id = null, int limit = 50)
        {
            var builder = new QueryStringBuilder();

            if (payout_id.HasValue)
            {
                builder.Add("payout_id", payout_id.Value);
            }
            if (since_id.HasValue)
            {
                builder.Add("since_id", since_id.Value);
            }

            builder.Add("limit", limit);
            builder.Add("order", "created asc");

            var queryString = builder.ToString();

            var path = $"/admin/shopify_payments/balance/transactions.json?{queryString}";
            var clientResponse = _executionFacade.Get(path);

            var output = clientResponse.Body;
            return output;
        }        
    }
}

