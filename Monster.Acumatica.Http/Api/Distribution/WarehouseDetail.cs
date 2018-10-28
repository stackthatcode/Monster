using System.Collections.Generic;
using Monster.Acumatica.Api.Common;


namespace Monster.Acumatica.Api.Distribution
{
    
    public class WarehouseDetail
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public StringValue DefaultIssueLocationID { get; set; }
        public StringValue DefaultReceiptLocationID { get; set; }
        public StringValue InventoryAccount { get; set; }
        public BoolValue IsDefault { get; set; }
        public BoolValue Override { get; set; }
        public BoolValue OverridePreferredVendor { get; set; }
        public BoolValue OverrideStdCost { get; set; }
        public StringValue PreferredVendor { get; set; }
        public BoolValue PriceOverride { get; set; }
        public StringValue ProductManager { get; set; }
        public StringValue ProductWorkgroup { get; set; }
        public DoubleValue QtyOnHand { get; set; }
        public StringValue ServiceLevel { get; set; }
        public StringValue Status { get; set; }
        public StringValue WarehouseID { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }
}
