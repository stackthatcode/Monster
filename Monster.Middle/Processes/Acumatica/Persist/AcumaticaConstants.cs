﻿namespace Monster.Middle.Processes.Acumatica.Persist
{
    public class AcumaticaConstants
    {
        public const string SalesOrderType = "SO";
        public const string CreditMemoType = "CM";
        
        public const string ShipmentType = "Shipment";

        public const string StatusOpen = "Open";
        public const string StatusCompleted = "Completed";
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
