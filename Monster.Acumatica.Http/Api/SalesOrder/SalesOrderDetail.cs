using System.Collections.Generic;
using Monster.Acumatica.Api.Common;


namespace Monster.Acumatica.Api.SalesOrder
{
    public class SalesOrderDetail
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public StringValue Account { get; set; }
        public StringValue AlternateID { get; set; }
        public BoolValue AutoCreateIssue { get; set; }
        public DoubleValue AverageCost { get; set; }
        public StringValue Branch { get; set; }
        public BoolValue Commissionable { get; set; }
        public BoolValue Completed { get; set; }
        public DoubleValue DiscountAmount { get; set; }
        public DoubleValue DiscountedUnitPrice { get; set; }
        public DoubleValue DiscountPercent { get; set; }
        public DoubleValue ExtendedPrice { get; set; }
        public BoolValue FreeItem { get; set; }
        public StringValue InventoryID { get; set; }
        public DateValue LastModifiedDate { get; set; }
        public StringValue LineDescription { get; set; }
        public IntegerValue LineNbr { get; set; }
        public StringValue LineType { get; set; }
        public StringValue Location { get; set; }
        public DoubleValue OpenQty { get; set; }
        public StringValue Operation { get; set; }
        public DoubleValue OrderQty { get; set; }
        public DoubleValue OvershipThreshold { get; set; }
        public DoubleValue QtyOnShipments { get; set; }
        public StringValue ReasonCode { get; set; }
        public DateValue RequestedOn { get; set; }
        public StringValue SalespersonID { get; set; }
        public DateValue ShipOn { get; set; }
        public StringValue ShippingRule { get; set; }
        public StringValue TaxCategory { get; set; }
        public DoubleValue UnbilledAmount { get; set; }
        public DoubleValue UndershipThreshold { get; set; }
        public DoubleValue UnitCost { get; set; }
        public DoubleValue UnitPrice { get; set; }
        public StringValue UOM { get; set; }
        public StringValue WarehouseID { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }
}
