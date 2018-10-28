using System.Collections.Generic;
using Monster.Acumatica.Api.Common;
using Push.Foundation.Utilities.Json;

namespace Monster.Acumatica.Api.Distribution
{
    public class InventoryReceipt
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public DoubleValue ControlCost { get; set; }
        public DoubleValue ControlQty { get; set; }
        public DateValue Date { get; set; }
        public StringValue Description { get; set; }
        public BoolValue Hold { get; set; }
        public StringValue PostPeriod { get; set; }
        public StringValue ReferenceNbr { get; set; }
        public StringValue Status { get; set; }
        public DoubleValue TotalCost { get; set; }
        public DoubleValue TotalQty { get; set; }
        public StringValue TransferNbr { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }

        public List<InventoryReceiptDetails> Details { get; set; }
    }

    public class InventoryReceiptDetails
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public StringValue Description { get; set; }
        public DoubleValue ExtCost { get; set; }
        public StringValue InventoryID { get; set; }
        public IntegerValue LineNumber { get; set; }
        public StringValue Location { get; set; }
        public DoubleValue Qty { get; set; }
        public DoubleValue UnitCost { get; set; }
        public StringValue UOM { get; set; }
        public StringValue WarehouseID { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }

    public class ReleaseInventoryReceipt
    {
        public Action entity { get; set; }

        public static ReleaseInventoryReceipt Build(string refNumber)
        {
            var output = new ReleaseInventoryReceipt
            {
                entity = new Action()
                {
                    Type = "Receipt".ToValue(),
                    ReferenceNbr = refNumber.ToValue(),
                }
            };
            return output;
        }
    }
}
