namespace Push.Shopify.Api.Order
{
    public class BillingAddress
    {
        public string first_name { get; set; }
        public string address1 { get; set; }
        public string phone { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string last_name { get; set; }
        public object address2 { get; set; }
        public string company { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public string name { get; set; }
        public string country_code { get; set; }
        public string province_code { get; set; }
    }
}