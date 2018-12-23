using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Reference
{ 
    public class AllowedCashAccount
    {
        public BoolValue APDefault { get; set; }
        public StringValue APLastRefNbr { get; set; }
        public BoolValue APSuggestNextNbr { get; set; }
        public BoolValue ARDefault { get; set; }
        public BoolValue ARDefaultForRefund { get; set; }
        public StringValue ARLastRefNbr { get; set; }
        public BoolValue ARSuggestNextNbr { get; set; }
        public StringValue BatchLastRefNbr { get; set; }
        public StringValue Branch { get; set; }
        public StringValue CashAccount { get; set; }
        public StringValue Description { get; set; }
        public StringValue PaymentMethod { get; set; }
        public BoolValue UseInAP { get; set; }
        public BoolValue UseInAR { get; set; }
        public string id { get; set; }
        public string note { get; set; }
        public int rowNumber { get; set; }
    }
}
