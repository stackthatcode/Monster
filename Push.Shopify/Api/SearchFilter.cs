using System;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Http;

namespace Push.Shopify.Api
{
    public class SearchFilter
    {
        public int? Limit { get; set; }
        public long? SinceId { get; set; }
        public DateTime? CreatedAtMinUtc { get; set; }
        public DateTime? CreatedAtMaxUtc { get; set; }
        public DateTime? UpdatedAtMinUtc { get; set; }
        public DateTime? UpdatedAtMaxUtc { get; set; }
        public string OrderBy { get; set; }
        public string Status { get; set; }
        

        public SearchFilter()
        {
            Status = "any";
            Limit = 250;
        }

        public SearchFilter Clone()
        {
            return new SearchFilter
            {
                Limit = this.Limit,
                SinceId = this.SinceId,
                CreatedAtMinUtc = this.CreatedAtMinUtc,
                CreatedAtMaxUtc = this.CreatedAtMaxUtc,
                UpdatedAtMinUtc = this.UpdatedAtMinUtc,
                UpdatedAtMaxUtc = this.UpdatedAtMaxUtc,
                OrderBy = this.OrderBy,
            };
        }

        
        public QueryStringBuilder ToQueryStringBuilder()
        {
            var builder = new QueryStringBuilder();
            
            if (Status != null)
            {
                builder.Add("status", Status);
            }
            if (Limit != null)
            {
                builder.Add("limit", Limit.Value);
            }
            if (SinceId != null)
            {
                builder.Add("since_id", SinceId.Value);
            }
            if (OrderBy != null)
            {
                builder.Add("order", OrderBy);
            }
            if (CreatedAtMinUtc != null)
            {
                builder.Add(
                    "created_at_min", CreatedAtMinUtc.Value.ToIso8601Utc());
            }
            if (CreatedAtMaxUtc != null)
            {
                builder.Add(
                    "created_at_max", CreatedAtMaxUtc.Value.ToIso8601Utc());
            }
            if (UpdatedAtMinUtc != null)
            {
                builder.Add(
                    "updated_at_min", UpdatedAtMinUtc.Value.ToIso8601Utc());
            }
            if (UpdatedAtMaxUtc != null)
            {
                builder.Add(
                    "updated_at_max", UpdatedAtMaxUtc.Value.ToIso8601Utc());
            }

            return builder;
        }

        public string ToQueryString()
        {
            return ToQueryStringBuilder().ToString();
        }
    }
}
