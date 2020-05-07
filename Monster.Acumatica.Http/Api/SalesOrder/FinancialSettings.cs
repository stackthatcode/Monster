using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class FinancialSettings
    {
        public BoolValue OverrideTaxZone { get; set; }
        public StringValue CustomerTaxZone { get; set; }
        public StringValue Branch { get; set; }
    }
}
