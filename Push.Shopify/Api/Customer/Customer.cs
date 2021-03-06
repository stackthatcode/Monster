using System;
using System.Collections.Generic;

namespace Push.Shopify.Api.Customer
{
    public class Customer
    {
        public long id { get; set; }
        public string email { get; set; }
        public bool accepts_marketing { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int orders_count { get; set; }
        public string state { get; set; }
        public string total_spent { get; set; }
        public string last_order_id { get; set; }
        public string note { get; set; }
        public bool verified_email { get; set; }
        public string multipass_identifier { get; set; }
        public bool tax_exempt { get; set; }
        public string phone { get; set; }
        public string tags { get; set; }
        public string last_order_name { get; set; }
        public Address default_address { get; set; }
        public List<Address> addresses { get; set; }
    }

    public class CustomerParent
    {
        public Customer customer { get; set; }
    }

    public class CustomerList
    {
        public List<Customer> customers { get; set; }
    }
}
