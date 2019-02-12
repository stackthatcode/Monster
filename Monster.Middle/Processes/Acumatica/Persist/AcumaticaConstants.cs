namespace Monster.Middle.Processes.Acumatica.Persist
{
    public class AcumaticaConstants
    {        
        public const string ShipmentType = "Shipment";
    }

    public class SalesOrderExpand
    {
        public const string TaxDetails = "TaxDetails";
        public const string Shipments = "Shipments";
    }

    public class Status
    {
        public const string Open = "Open";
        public const string BackOrder = "Back Order";
        public const string Completed = "Completed";
    }

    public class SalesOrderType
    {
        public const string SO = "SO";
        public const string CM = "CM";
    }

    public class SalesInvoiceType
    {
        public const string Credit_Memo = "Credit Memo";
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
