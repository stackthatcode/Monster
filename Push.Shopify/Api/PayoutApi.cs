using System;
using Push.Foundation.Utilities.Http;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Shopify.Http;


namespace Push.Shopify.Api
{
    public class PayoutApi
    {
        private readonly ShopifyHttpContext _httpClient;
        private readonly IPushLogger _logger;

        public PayoutApi(
                IPushLogger logger, 
                ShopifyHttpContext httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public virtual string RetrievePayoutsHeaders(
                    DateTime minDate, int page = 1, int limit = 50)
        {
            var queryString 
                = new QueryStringBuilder()
                    .Add("min_date", minDate.ToString("YYYY-mm-dd"))
                    .Add("page", page)
                    .Add("limit", limit)
                    .ToString();

            var path = $"/admin/shopify_payments/payouts.json?{queryString}";
            var clientResponse = _httpClient.Get(path);

            var output = clientResponse.Body;
            return output;
        }

        public virtual string RetrievePayoutHeader(long payout_id)
        {

            var path = $"/admin/shopify_payments/payouts/{payout_id}.json";
            var clientResponse = _httpClient.Get(path);

            var output = clientResponse.Body;
            return output;
        }

        public virtual string 
                RetrievePayoutDetail(long? payout_id = null, long? since_id = null, int limit = 50)
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

