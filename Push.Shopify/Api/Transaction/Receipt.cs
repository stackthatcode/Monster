using System;

namespace Push.Shopify.Api.Transaction
{
    public class Receipt
    {
        public DateTimeOffset? timestamp { get; set; }
        public string ack { get; set; }
        public string correlation_id { get; set; }
        public string version { get; set; }
        public string build { get; set; }
        public string token { get; set; }
        public string transaction_id { get; set; }
        public string parent_transaction_id { get; set; }
        public string receipt_id { get; set; }
        public string transaction_type { get; set; }
        public string payment_type { get; set; }
        public DateTimeOffset payment_date { get; set; }
        public string gross_amount { get; set; }
        public string gross_amount_currency_id { get; set; }
        public string tax_amount { get; set; }
        public string tax_amount_currency_id { get; set; }
        public string exchange_rate { get; set; }
        public string payment_status { get; set; }
        public string pending_reason { get; set; }
        public string reason_code { get; set; }
        public string protection_eligibility { get; set; }
        public string protection_eligibility_type { get; set; }
        public string secure_merchant_account_id { get; set; }
        public string success_page_redirect_requested { get; set; }
        public string coupled_payment_info { get; set; }

        // PayPal specific
        public string Token { get; set; }
        public PaymentInfo PaymentInfo { get; set; }
        public string SuccessPageRedirectRequested { get; set; }
        public string CoupledPaymentInfo { get; set; }

        public string authorization_id { get; set; }
        public string fee_amount { get; set; }
        public string fee_amount_currency_id { get; set; }
        public string shipping_method { get; set; }

        public string AuthorizationID { get; set; }
    }

}
