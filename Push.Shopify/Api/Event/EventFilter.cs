using System;
using Push.Foundation.Web.Helpers;

namespace Push.Shopify.Api.Event
{
    public class EventFilter
    {
        public DateTime? CreatedAtMinUtc { get; set; }
        public string Verb { get; set; }
        public string Filter { get; set; }
        public int? Page { get; set; }
        public int? Limit { get; set; }


        public EventFilter()
        {
            CreatedAtMinUtc = DateTime.UtcNow;
            Limit = 250;
            Page = 1;
        }

        public EventFilter Clone()
        {
            return new EventFilter
            {
                CreatedAtMinUtc = this.CreatedAtMinUtc,
                Verb = this.Verb,
                Filter = this.Filter,
                Page = this.Page,
                Limit = this.Limit,
            };
        }
        public override string ToString()
        {
            return $"Event Filter dump: CreatedAtMin: {CreatedAtMinUtc} - Method: {Verb} - Filter: {Filter}";
        }

        public QueryStringBuilder ToQueryStringBuilder()
        {
            var builder = new QueryStringBuilder();

            if (CreatedAtMinUtc != null)
            {
                builder.Add("created_at_min", CreatedAtMinUtc.Value);
            }
            if (Verb != null)
            {
                builder.Add("verb", Verb);
            }
            if (Filter != null)
            {
                builder.Add("filter", Filter);
            }
            if (Page != null)
            {
                builder.Add("page", Page.Value);
            }
            if (Limit != null)
            {
                builder.Add("limit", Limit.Value);
            }

            return builder;
        }

        public string ToQueryString()
        {
            return ToQueryStringBuilder().ToString();
        }
    }
}
