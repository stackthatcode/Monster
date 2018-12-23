using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Reference
{
    public class Tax
    {
        public StringValue CalculateOn { get; set; }
        public StringValue CashDiscount { get; set; }
        public DateValue CreatedDateTime { get; set; }
        public StringValue Description { get; set; }
        public BoolValue EnterFromTaxBill { get; set; }
        public BoolValue ExcludeFromTaxonTaxCalculation { get; set; }
        public DateValue LastModifiedDateTime { get; set; }
        public DateValue NotValidAfter { get; set; }
        public StringValue TaxAgency { get; set; }
        public StringValue TaxClaimableAccount { get; set; }
        public StringValue TaxExpenseAccount { get; set; }
        public StringValue TaxID { get; set; }
        public StringValue TaxPayableAccount { get; set; }
        StringValue TaxType { get; set; }
        public string id { get; set; }
        public string note { get; set; }
        public int rowNumber { get; set; }        
    }
}
