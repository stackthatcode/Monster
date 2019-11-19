using System;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Http;
using Push.Foundation.Web.Helpers;

namespace Push.Shopify.Api
{
    public class SearchFilter
    {
        public int? Page { get; set; }
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
            Page = 1;
            Limit = 250;
        }

        public SearchFilter Clone()
        {
            return new SearchFilter
            {
                Page = this.Page,
                Limit = this.Limit,
                SinceId = this.SinceId,
                CreatedAtMinUtc = this.CreatedAtMinUtc,
                CreatedAtMaxUtc = this.CreatedAtMaxUtc,
                UpdatedAtMinUtc = this.UpdatedAtMinUtc,
                UpdatedAtMaxUtc = this.UpdatedAtMaxUtc,
                OrderBy = this.OrderBy,
            };
        }

        public void OrderByCreatedAt()
        {
            this.OrderBy = "created_at asc";
        }

        public void OrderByUpdatedAt()
        {
            this.OrderBy = "updated_at asc";
        }
        
        public QueryStringBuilder ToQueryStringBuilder()
        {
            var builder = new QueryStringBuilder();
            
            if (Status != null)
            {
                builder.Add("status", Status);
            }
            if (Page != null)
            {
                builder.Add("page", Page.Value);
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
