using System;
using System.Collections.Generic;

namespace Push.Shopify.Api.Order
{
    public class Fulfillment
    {
        public long id { get; set; }
        public long order_id { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }
        public string service { get; set; }
        public DateTime updated_at { get; set; }
        public string tracking_company { get; set; }
        public string shipment_status { get; set; }
        public long location_id { get; set; }
        public string tracking_number { get; set; }
        public List<string> tracking_numbers { get; set; }
        public string tracking_url { get; set; }
        public List<string> tracking_urls { get; set; }
        public Receipt receipt { get; set; }
        public string name { get; set; }

        public List<LineItem> line_items { get; set; }
    }

    public class FulfillmentParent
    {
        public Fulfillment fulfillment { get; set; }
    }
}
