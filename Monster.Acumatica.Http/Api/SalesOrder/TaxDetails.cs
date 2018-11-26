using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class TaxDetails
    {
        public string id { get; set; }
        public StringValue TaxID { get; set; }
        public DoubleValue TaxAmount { get; set; }
        public DoubleValue TaxRate { get; set; }
        public DoubleValue TaxableAmount { get; set; }
    }
}
