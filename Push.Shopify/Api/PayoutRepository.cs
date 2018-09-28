using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Shopify.Http;


namespace Push.Shopify.Api
{
    public class PayoutRepository
    {
        private readonly ShopifyHttpContext _httpClient;
        private readonly IPushLogger _logger;

        public PayoutRepository(
                IPushLogger logger, 
                ShopifyHttpContext httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public virtual string RetrievePayouts(int limit = 50, int page = 1)
        {
            var queryString 
                = new QueryStringBuilder()
                    .Add("limit", limit)
                    .Add("page", page)
                    .ToString();

            var path = $"/admin/shopify_payments/payouts.json?{queryString}";
            var clientResponse = _httpClient.Get(path);

            var output = clientResponse.Body;
            return output;
        }

        public virtual string 
                RetrievePayoutDetail(
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
            var clientResponse = _httpClient.Get(path);

            _logger.Debug($"{clientResponse.Body}");

            var output = clientResponse.Body;
            return output;
        }        
    }
}

