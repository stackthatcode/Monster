using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Cash
{    
    public class ImportBankTransaction
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }

        public IntegerValue LineNbr { get; set; }
        public StringValue AppliedRule { get; set; }
        public DoubleValue BeginningBalance { get; set; }
        public StringValue BusinessAccount { get; set; }
        public StringValue BusinessAccountName { get; set; }
        public DoubleValue CalculatedBalance { get; set; }
        public StringValue CashAccount { get; set; }
        public IntegerValue CashAccountID { get; set; }
        public DoubleValue Disbursement { get; set; }
        public BoolValue DocumentMatched { get; set; }
        public DateValue EndBalanceDate { get; set; }
        public DoubleValue EndingBalance { get; set; }
        public StringValue EntryTypeID { get; set; }
        public StringValue ExtRefNbr { get; set; }
        public StringValue ExtTranID { get; set; }
        public BoolValue Hidden { get; set; }
        public StringValue InvoiceNbr { get; set; }
        public StringValue Location { get; set; }
        public StringValue Module { get; set; }
        public StringValue PayeeName { get; set; }
        public StringValue PaymentMethod { get; set; }
        public BoolValue Processed { get; set; }
        public DoubleValue Receipt { get; set; }
        public StringValue ReferenceNbr { get; set; }
        public DateValue StartBalanceDate { get; set; }
        public DateValue StatementDate { get; set; }
        public StringValue TranCode { get; set; }
        public DateValue TranDate { get; set; }
        public StringValue TranDesc { get; set; }
        public StringValue TranID { get; set; }
        public StringValue custom { get; set; }
        public List<string> files { get; set; }
    }
}
