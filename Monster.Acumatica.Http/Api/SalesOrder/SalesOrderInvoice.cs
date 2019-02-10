using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class SalesOrderInvoice
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public StringValue BaseCurrencyID { get; set; }
        public DoubleValue ControlQty { get; set; }
        public DateValue CreatedDateTime { get; set; }
        public StringValue CurrencyID { get; set; }
        public DoubleValue CurrencyRate { get; set; }
        public StringValue CurrencyRateTypeID { get; set; }
        public StringValue CurrencyViewState { get; set; }
        public StringValue CustomerID { get; set; }
        public List<SalesOrderInvoiceDetail> Details { get; set; }
        public DateValue EffectiveDate { get; set; }
        public StringValue FOBPoint { get; set; }
        public DoubleValue FreightAmount { get; set; }
        public DoubleValue FreightCost { get; set; }
        public StringValue FreightCurrency { get; set; }
        public BoolValue GroundCollect { get; set; }
        public BoolValue Hold { get; set; }
        public BoolValue Insurance { get; set; }
        public DateValue LastModifiedDateTime { get; set; }
        public StringValue Operation { get; set; }
        public StringValue Owner { get; set; }
        public StringValue PackageCount { get; set; }
        public DoubleValue PackageWeight { get; set; }
        public DoubleValue ReciprocalRate { get; set; }
        public BoolValue ResidentialDelivery { get; set; }
        public BoolValue SaturdayDelivery { get; set; }
        public DateValue ShipmentDate { get; set; }
        public StringValue ShipmentNbr { get; set; }
        public DoubleValue ShippedQty { get; set; }
        public DoubleValue ShippedVolume { get; set; }
        public DoubleValue ShippedWeight { get; set; }
    //    public ShippingTerms ShippingTerms { get; set; }
    //    public ShippingZoneID ShippingZoneID { get; set; }
    //    public ShipVia ShipVia { get; set; }
    //    public Status Status { get; set; }
    //    public ToWarehouseID ToWarehouseID { get; set; }
    //    public Type Type { get; set; }
    //    public WarehouseID2 WarehouseID { get; set; }
    //    public WorkgroupID WorkgroupID { get; set; }
    //    public Custom2 custom { get; set; }
    //    public List<object> files { get; set; }
    }

    public class SalesOrderInvoiceDetail
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
        public IntegerValue OrderLineNbr { get; set; }
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

