using System.Collections.Generic;
using Newtonsoft.Json;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Shopify.Api.Event;
using Push.Shopify.Http;


namespace Push.Shopify.Api
{
    public class EventApi
    {
        private readonly ShopifyHttpContext _httpClient;
        private readonly IPushLogger _logger;
        
        public EventApi(
                IPushLogger logger, ShopifyHttpContext httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }
        

        public virtual string
                    Retrieve(EventFilter filter, int page = 1, int limit = 250)
        {
            var querystring
                = new QueryStringBuilder()
                    .Add("page", page)
                    .Add("limit", limit)
                    .Add(filter.ToQueryStringBuilder())
                    .ToString();

            var url = "/admin/events.json?" + querystring;
            var clientResponse = _httpClient.Get(url);

            return clientResponse.Body;
        }
    }
}

