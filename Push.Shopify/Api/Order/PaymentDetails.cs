namespace Push.Shopify.Api.Order
{
    public class PaymentDetails
    {
        public string credit_card_bin { get; set; }
        public object avs_result_code { get; set; }
        public object cvv_result_code { get; set; }
        public string credit_card_number { get; set; }
        public string credit_card_company { get; set; }
    }
}