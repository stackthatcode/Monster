using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Push.Shopify.Api.Order
{
    public class ShippingLine
    {
        public long id { get; set; }
        public string title { get; set; }
        public string code { get; set; }
        public string source { get; set; }
        public string phone { get; set; }
        public string requested_fulfillment_service_id { get; set; }
        public string delivery_category { get; set; }
        public string carrier_identifier { get; set; }

        public decimal price { get; set; }
        public decimal discounted_price { get; set; }

        public List<DiscountAllocation> discount_allocations { get; set; }
        public List<TaxLine> tax_lines { get; set; }


        // Computed fields
        [JsonIgnore]
        public decimal TotalTaxes => tax_lines.Sum(x => x.rate);        
    }
}

