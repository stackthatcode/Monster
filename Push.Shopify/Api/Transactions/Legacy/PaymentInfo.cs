using System;

namespace Push.Shopify.Api.Transactions.Legacy
{
    [Obsolete]
    public class PaymentInfo
    {
        public string TransactionID { get; set; }
        public string ParentTransactionID { get; set; }
        public object ReceiptID { get; set; }
        public string TransactionType { get; set; }
        public string PaymentType { get; set; }
        public DateTime PaymentDate { get; set; }
        public string GrossAmount { get; set; }
        public string TaxAmount { get; set; }
        public object ExchangeRate { get; set; }
        public string PaymentStatus { get; set; }
        public string PendingReason { get; set; }
        public string ReasonCode { get; set; }
        public string ProtectionEligibility { get; set; }
        public string ProtectionEligibilityType { get; set; }
        public SellerDetails SellerDetails { get; set; }
        public string FeeAmount { get; set; }
        public string ShippingMethod { get; set; }
    }
}
