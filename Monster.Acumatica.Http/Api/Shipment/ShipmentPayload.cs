using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Shipment
{
    public class ShipmentPayload
    {
        public StringValue ShipmentNbr { get; set; }
        public StringValue Type { get; set; }
        public StringValue Status { get; set; }

        public ShipmentPayload(string shipmentNbr)
        {
            ShipmentNbr = new StringValue(shipmentNbr);
            Type = new StringValue("Shipment");
        }

        public ShipmentPayload()
        {
        }
    }
}
