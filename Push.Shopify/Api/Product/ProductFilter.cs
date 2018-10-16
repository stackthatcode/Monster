using System;
using Push.Foundation.Web.Helpers;

namespace Push.Shopify.Api.Product
{

    public class ProductFilter
    {
        public int? Page { get; set; }
        public int? Limit { get; set; }
        public long? SinceId { get; set; }
        public DateTime? CreatedAtMinUtc { get; set; }
        public DateTime? CreatedAtMaxUtc { get; set; }
        public DateTime? UpdatedAtMinUtc { get; set; }
        public DateTime? UpdatedAtMaxUtc { get; set; }


        public ProductFilter()
        {
            Page = 1;
            Limit = 250;
        }

        public ProductFilter Clone()
        {
            return new ProductFilter
            {
                Page = this.Page,
                Limit = this.Limit,
                SinceId = this.SinceId,
                CreatedAtMinUtc = this.CreatedAtMinUtc,
                CreatedAtMaxUtc = this.CreatedAtMaxUtc,
                UpdatedAtMinUtc = this.UpdatedAtMinUtc,
                UpdatedAtMaxUtc = this.UpdatedAtMaxUtc,
            };
        }

        public QueryStringBuilder ToQueryStringBuilder()
        {
            var builder = new QueryStringBuilder();

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
            if (CreatedAtMinUtc != null)
            {
                builder.Add("created_at_min", CreatedAtMinUtc.Value);
            }
            if (CreatedAtMaxUtc != null)
            {
                builder.Add("created_at_max", CreatedAtMinUtc.Value);
            }
            if (UpdatedAtMinUtc != null)
            {
                builder.Add("updated_at_min", UpdatedAtMinUtc.Value);
            }
            if (UpdatedAtMaxUtc != null)
            {
                builder.Add("updated_at_max", UpdatedAtMaxUtc.Value);
            }

            return builder;
        }

        public string ToQueryString()
        {
            return ToQueryStringBuilder().ToString();
        }
    }
}
