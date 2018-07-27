using System.Collections.Generic;

namespace Push.Shopify.Api.Order
{
    public class ShippingLine
    {
        public long id { get; set; }
        public string title { get; set; }
        public string price { get; set; }
        public string code { get; set; }
        public string source { get; set; }
        public object phone { get; set; }
        public object requested_fulfillment_service_id { get; set; }
        public object delivery_category { get; set; }
        public object carrier_identifier { get; set; }
        public string discounted_price { get; set; }
        public List<object> discount_allocations { get; set; }
        public List<TaxLine> tax_lines { get; set; }
    }
}