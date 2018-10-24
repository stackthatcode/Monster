using System.Collections.Generic;
using Monster.Acumatica.Api.Common;


namespace Monster.Acumatica.Api.Distribution
{
    public class Warehouse
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public BoolValue Active { get; set; }
        public StringValue COGSExpenseAccount { get; set; }
        public StringValue Description { get; set; }
        public StringValue DiscountAccount { get; set; }
        public StringValue FreightChargeAccount { get; set; }
        public StringValue InventoryAccount { get; set; }
        public StringValue LandedCostVarianceAccount { get; set; }
        public StringValue MiscChargeAccount { get; set; }
        public StringValue OverrideInventoryAccountSubaccount { get; set; }
        public StringValue POAccrualAccount { get; set; }
        public StringValue PurchasePriceVarianceAccount { get; set; }
        public StringValue ReceivingLocationID { get; set; }
        public StringValue RMALocationID { get; set; }
        public StringValue SalesAccount { get; set; }
        public StringValue ShippingLocationID { get; set; }
        public StringValue StandardCostRevaluationAccount { get; set; }
        public StringValue StandardCostVarianceAccount { get; set; }
        public StringValue WarehouseID { get; set; }
        public StringValue custom { get; set; }

        public List<WarehouseLocation> Locations { get; set; }
    }

}
