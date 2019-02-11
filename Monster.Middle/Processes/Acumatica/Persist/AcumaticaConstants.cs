namespace Monster.Middle.Processes.Acumatica.Persist
{
    public class AcumaticaConstants
    {        
        public const string ShipmentType = "Shipment";

        public const string StatusOpen = "Open";
        public const string StatusBackOrder = "Back Order";
        public const string StatusCompleted = "Completed";
    }

    public class SalesOrderType
    {
        public const string SO = "SO";
        public const string CM = "CM";
    }


    public class ShipmentStatus
    {
        public const string Open = "Open";
        public const string Confirmed = "Completed";
    }

    public class PaymentType
    {
        public const string Payment = "Payment";
        public const string CustomerRefund = "Customer Refund";
    }
}
