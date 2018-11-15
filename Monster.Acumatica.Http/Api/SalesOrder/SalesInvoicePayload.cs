using System;
using System.Collections.Generic;
using Monster.Acumatica.Api.Common;


namespace Monster.Acumatica.Api.SalesOrder
{
    // Currently purposed only for writing invoices
    //
    public class SalesInvoicePayload
    {
        public StringValue Type { get; set; }
        public StringValue ReferenceNbr { get; set; }
        public StringValue Status { get; set; }
        public StringValue CustomerID { get; set; }

        public List<SalesInvoiceDetailsPayload> Details { get; set; }

        public SalesInvoicePayload()
        {
            Details = new List<SalesInvoiceDetailsPayload>();
        }
    }

    public class SalesInvoiceDetailsPayload
    {
        public StringValue ShipmentNbr { get; set; }
        public StringValue OrderType { get; set; }
        public StringValue OrderNbr { get; set; }
    }
}
