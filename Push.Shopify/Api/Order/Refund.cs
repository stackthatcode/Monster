using System;
using System.Collections.Generic;

namespace Push.Shopify.Api.Order
{
    public class Refund
    {
        public long id { get; set; }
        public long order_id { get; set; }
        public DateTime created_at { get; set; }
        public string note { get; set; }
        public long? user_id { get; set; }
        public DateTime processed_at { get; set; }
        public bool restock { get; set; }

        public List<RefundLineItem> refund_line_items { get; set; }
        public List<Transaction> transactions { get; set; }
        public List<OrderAdjustment> order_adjustments { get; set; }
    }
}