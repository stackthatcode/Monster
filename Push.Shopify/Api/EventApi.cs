using Push.Foundation.Utilities.Http;
using Push.Shopify.Api.Event;
using Push.Shopify.Http;

namespace Push.Shopify.Api
{
    public class EventApi
    {
        private readonly ShopifyHttpContext _httpClient;
        
        public EventApi(ShopifyHttpContext httpClient)
        {
            _httpClient = httpClient;
        }
        
        public ResponseEnvelope Retrieve(EventFilter filter, int limit = 250)
        {
            var querystring
                = new QueryStringBuilder()
                    .Add("limit", limit)
                    .Add(filter.ToQueryStringBuilder())
                    .ToString();

            var url = "/admin/api/2019-10/events.json?" + querystring;
            return _httpClient.Get(url);
        }

        public ResponseEnvelope RetrieveByLink(string link)
        {
            return _httpClient.Get(link);
        }
    }
}

