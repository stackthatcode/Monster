using System;
using System.Collections.Generic;

namespace Push.Shopify.Api.Payout
{

    public class PayoutTransaction
    {
        public long id { get; set; }
        public string type { get; set; }
        public bool test { get; set; }
        public long? payout_id { get; set; }
        public string payout_status { get; set; }
        public string currency { get; set; }
        public double amount { get; set; }
        public double fee { get; set; }
        public double net { get; set; }
        public long source_id { get; set; }
        public string source_type { get; set; }
        public long? source_order_id { get; set; }
        public long? source_order_transaction_id { get; set; }
        public DateTime processed_at { get; set; }
    }

    public class PayoutDetail
    {
        public List<PayoutTransaction> transactions { get; set; }
    }
}

