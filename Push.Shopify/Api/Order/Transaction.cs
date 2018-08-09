using System;

namespace Push.Shopify.Api.Order
{
    public class Transaction
    {
        public long id { get; set; }
        public long order_id { get; set; }
        public decimal amount { get; set; }
        public string kind { get; set; }
        public string gateway { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public DateTime created_at { get; set; }
        public bool test { get; set; }
        public string authorization { get; set; }
        public string currency { get; set; }
        public long? location_id { get; set; }
        public long? user_id { get; set; }
        public long parent_id { get; set; }
        public long? device_id { get; set; }
        public Receipt receipt { get; set; }
        public string error_code { get; set; }
        public string source_name { get; set; }
        

        public bool IsSuccess => status == "success";
    }
}