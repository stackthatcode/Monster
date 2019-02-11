using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class SalesInvoice
    {
        public DoubleValue Amount { get; set; }
        public DoubleValue Balance { get; set; }
        // public object BillingSettings { get; set; }
        public StringValue CustomerID { get; set; }
        public StringValue CustomerOrder { get; set; }
        public DateValue Date { get; set; }
        public StringValue Description { get; set; }
        public IList<SalesInvoiceDetail> Details { get; set; }
        public DateValue DueDate { get; set; }
        public BoolValue Hold { get; set; }
        public StringValue ReferenceNbr { get; set; }
        public StringValue Status { get; set; }
        public StringValue Type { get; set; }
    }

    public class SalesInvoiceDetail
    {       
    }
}

