using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class ShippingSettings
    {
        public BoolValue ShipSeparately { get; set; }
        public StringValue ShippingRule { get; set; }
    }
}
