using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Shipment
{

    public class ShipmentDetail
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public StringValue Description { get; set; }
        public BoolValue FreeItem { get; set; }
        public StringValue InventoryID { get; set; }
        public IntegerValue LineNbr { get; set; }
        public StringValue LocationID { get; set; }
        public DoubleValue OpenQty { get; set; }
        public DoubleValue OrderedQty { get; set; }
        public StringValue OrderLineNbr { get; set; }
        public StringValue OrderNbr { get; set; }
        public StringValue OrderType { get; set; }
        public DoubleValue OriginalQty { get; set; }
        public StringValue ReasonCode { get; set; }
        public DoubleValue ShippedQty { get; set; }
        public StringValue UOM { get; set; }
        public StringValue WarehouseID { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }

}
