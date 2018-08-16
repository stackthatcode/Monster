using System;
using System.Collections.Generic;


namespace Push.Shopify.Api.Payout
{
    public class BalanceTransaction
    {
        public long id { get; set; }
        public string amount { get; set; }
        public string fee { get; set; }
        public string net { get; set; }
        public string transaction_id { get; set; }
        public string transaction_type { get; set; }
        public DateTimeOffset payout_date { get; set; }
        public string transfer_id { get; set; }
        public string payments_transfer_id { get; set; }
        public DateTimeOffset created { get; set; }
        public long order_id { get; set; }
        public string order_name { get; set; }
        public string payout_status { get; set; }
        public string displayed_transaction_type { get; set; }
        public string status { get; set; }
    }

    public class BalanceTransactionList
    {
        public List<BalanceTransaction> transactions { get; set; }
    }
}
