using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push.Shopify.Api.Event
{
    public class Event
    {
        public long id { get; set; }
        public long subject_id { get; set; }
        public DateTimeOffset created_at { get; set; }
        public string subject_type { get; set; }
        public string verb { get; set; }
        public List<string> arguments { get; set; }
        public object body { get; set; }
        public string message { get; set; }
        public string author { get; set; }
        public string description { get; set; }
        public string path { get; set; }
    }

    public class EventList
    {
        public List<Event> events { get; set; }
    }
}

