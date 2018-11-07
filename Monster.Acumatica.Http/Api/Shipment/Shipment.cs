using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Shipment
{

    public class Shipment
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
        public BoolValue CurrencyViewState { get; set; }
        public StringValue CustomerID { get; set; }
        public List<ShipmentDetail> Details { get; set; }
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
        public DoubleValue PackageCount { get; set; }
        public DoubleValue PackageWeight { get; set; }
        public DoubleValue ReciprocalRate { get; set; }
        public BoolValue ResidentialDelivery { get; set; }
        public BoolValue SaturdayDelivery { get; set; }
        public DateValue ShipmentDate { get; set; }
        public StringValue ShipmentNbr { get; set; }
        public DoubleValue ShippedQty { get; set; }
        public DoubleValue ShippedVolume { get; set; }
        public DoubleValue ShippedWeight { get; set; }
        public StringValue ShippingTerms { get; set; }
        public StringValue ShippingZoneID { get; set; }
        public StringValue ShipVia { get; set; }
        public StringValue Status { get; set; }
        public StringValue ToWarehouseID { get; set; }
        public StringValue Type { get; set; }
        public StringValue WarehouseID { get; set; }
        public StringValue WorkgroupID { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }
}
