using System;
using System.Collections.Generic;

namespace Push.Shopify.Api.Payout
{
    public class Summary
    {
        public decimal adjustments_fee_amount { get; set; }
        public decimal adjustments_gross_amount { get; set; }
        public decimal charges_fee_amount { get; set; }
        public decimal charges_gross_amount { get; set; }
        public decimal refunds_fee_amount { get; set; }
        public decimal refunds_gross_amount { get; set; }
        public decimal reserved_funds_fee_amount { get; set; }
        public decimal reserved_funds_gross_amount { get; set; }
        public decimal retried_payouts_fee_amount { get; set; }
        public decimal retried_payouts_gross_amount { get; set; }
    }
    
    public class Payout
    {
        public long id { get; set; }
        public string status { get; set; }
        public DateTime date { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }
        public Summary summary { get; set; }
    }

    public class PayoutList
    {
        public List<Payout> payouts { get; set; }
    }
}
