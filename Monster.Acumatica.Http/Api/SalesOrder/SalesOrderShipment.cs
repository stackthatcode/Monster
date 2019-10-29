using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class SalesOrderShipment
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public StringValue InventoryDocType { get; set; }
        public StringValue InventoryRefNbr { get; set; }
        public StringValue InvoiceNbr { get; set; }
        public StringValue InvoiceType { get; set; }
        public DateValue ShipmentDate { get; set; }
        public StringValue ShipmentNbr { get; set; }
        public StringValue ShipmentType { get; set; }
        public DoubleValue ShippedQty { get; set; }
        public DoubleValue ShippedVolume { get; set; }
        public DoubleValue ShippedWeight { get; set; }
        public StringValue Status { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }
}
