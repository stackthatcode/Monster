using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Shipment
{
    public class ShipmentConfirmation
    {
        public ShipmentConfirmationEntity entity { get; set; }

        public ShipmentConfirmation(string shipmentNbr)
        {
            entity = new ShipmentConfirmationEntity()
            {
                ShipmentNbr = new StringValue(shipmentNbr),
                Type = new StringValue("Shipment")
            };
        }

        public ShipmentConfirmation()
        {
        }
    }

    public class ShipmentConfirmationEntity
    {
        public StringValue ShipmentNbr { get; set; }
        public StringValue Type { get; set; }

    }
}
