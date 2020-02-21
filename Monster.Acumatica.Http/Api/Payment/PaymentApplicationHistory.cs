using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Payment
{
    public class PaymentApplicationHistory
    {
        public StringValue AdjustedDocType { get; set; }
        public StringValue AdjustedRefNbr { get; set; }
        public StringValue AdjustingDocType { get; set; }
        public StringValue AdjustingRefNbr { get; set; }
        public IntegerValue AdjustmentNbr { get; set; }
        public BoolValue AdjustsVAT { get; set; }
        public DoubleValue AmountPaid { get; set; }
        public StringValue ApplicationPeriod { get; set; }
        public DoubleValue Balance { get; set; }
        public DoubleValue BalanceWriteOff { get; set; }
        public StringValue BatchNbr { get; set; }
        public DoubleValue CashDiscountBalance { get; set; }
        public DateValue CashDiscountDate { get; set; }
        public DoubleValue CashDiscountTaken { get; set; }
        public StringValue CurrencyID { get; set; }
        public StringValue Customer { get; set; }
        public StringValue CustomerOrder { get; set; }
        public DateValue Date { get; set; }
        public StringValue Description { get; set; }
        public StringValue DisplayDocTyp { get; set; }
        public StringValue DisplayRefNbr { get; set; }
        public DateValue DueDate { get; set; }
        public StringValue PostPeriod { get; set; }
        public StringValue VATCreditMemo { get; set; }

}
}
