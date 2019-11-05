using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class SalesOrderTotals
    {
        public DoubleValue DiscountTotal { get; set; }
        public DoubleValue Freight { get; set; }
        public DoubleValue FreightCost { get; set; }
        public BoolValue FreightCostIsuptodate { get; set; }
        public StringValue FreightTaxCategory { get; set; }
        public DoubleValue LineTotalAmount { get; set; }
        public DoubleValue MiscTotalAmount { get; set; }
        public DoubleValue OrderVolume { get; set; }
        public DoubleValue OrderWeight { get; set; }
        public DoubleValue PackageWeight { get; set; }
        public DoubleValue PremiumFreight { get; set; }
        public DoubleValue TaxTotal { get; set; }
        public DoubleValue UnbilledAmount { get; set; }
        public DoubleValue UnbilledQty { get; set; }
        public DoubleValue UnpaidBalance { get; set; }
    }
}
