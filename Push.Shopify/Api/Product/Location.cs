using System;
using System.Collections.Generic;

namespace Push.Shopify.Api.Product
{
    public class Location
    {
        public long id { get; set; }
        public string name { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string phone { get; set; }
        public DateTimeOffset created_at { get; set; }
        public DateTimeOffset updated_at { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public string province_code { get; set; }
        public bool legacy { get; set; }
        public string admin_graphql_api_id { get; set; }
    }

    public class LocationList
    {
        public List<Location> locations { get; set; }
    }
}
