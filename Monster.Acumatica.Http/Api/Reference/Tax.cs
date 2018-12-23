using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Reference
{
    public class Tax
    {
        private StringValue CalculateOn { get; set; }
        StringValue CashDiscount { get; set; }
        DateValue CreatedDateTime { get; set; }
        StringValue Description { get; set; }
        BoolValue EnterFromTaxBill { get; set; }
        BoolValue ExcludeFromTaxonTaxCalculation { get; set; }
        DateValue LastModifiedDateTime { get; set; }
        DateValue NotValidAfter { get; set; }
        StringValue TaxAgency { get; set; }
        StringValue TaxClaimableAccount { get; set; }
        StringValue TaxExpenseAccount { get; set; }
        StringValue TaxID { get; set; }
        StringValue TaxPayableAccount { get; set; }
        StringValue TaxType { get; set; }
        string custom { get; set; }
        string id { get; set; }
        string note { get; set; }
        int rowNumber { get; set; }
        
    }
}
