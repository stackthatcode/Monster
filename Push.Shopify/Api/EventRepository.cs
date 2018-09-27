using System.Collections.Generic;
using Newtonsoft.Json;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Http;
using Push.Shopify.Api.Event;


namespace Push.Shopify.Api
{
    public class EventRepository
    {
        private readonly HttpFacade _client;
        private readonly IPushLogger _logger;
        
        public EventRepository(
                HttpFacade client,
                IPushLogger logger)
        {
            _client = client;
            _logger = logger;
        }
        
        public virtual int RetrieveCount(EventFilter filter)
        {
            var url = "/admin/events/count.json?" + filter.ToQueryStringBuilder();
            var clientResponse = _client.Get(url);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var count = parent.count;
            return count;
        }


        public virtual IList<Legacy.Event.Event> Retrieve(EventFilter filter, int page = 1, int limit = 250)
        {
            var querystring
                = new QueryStringBuilder()
                    .Add("page", page)
                    .Add("limit", limit)
                    .Add(filter.ToQueryStringBuilder())
                    .ToString();

            var url = "/admin/events.json?" + querystring;
            var clientResponse = _client.Get(url);

            _logger.Info(clientResponse.Body);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);

            var output = new List<Legacy.Event.Event>();
            foreach (var @event in parent.events)
            {
                var result = new Legacy.Event.Event();
                result.Id = @event.id;
                result.SubjectId = @event.subject_id;
                result.SubjectType = @event.subject_type;
                result.Verb = @event.verb;
                output.Add(result);
            }
            return output;
        }
    }
}

