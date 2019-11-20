using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
   
    public class Invoice
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public DoubleValue Amount { get; set; }
        public DoubleValue Balance { get; set; }
        public BoolValue BillingPrinted { get; set; }
        public DateValue CreatedDateTime { get; set; }
        public StringValue Customer { get; set; }
        public StringValue CustomerOrder { get; set; }
        public DateValue Date { get; set; }
        public StringValue Description { get; set; }
        public DateValue DueDate { get; set; }
        public BoolValue Hold { get; set; }
        public DateValue LastModifiedDateTime { get; set; }
        public StringValue LinkARAccount { get; set; }
        public StringValue PostPeriod { get; set; }
        public StringValue ReferenceNbr { get; set; }
        public StringValue Status { get; set; }
        public List<InvoiceTaxDetail> TaxDetails { get; set; }
        public DoubleValue TaxTotal { get; set; }
        public StringValue Terms { get; set; }
        public StringValue Type { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }

    public class InvoiceTaxDetail
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public object note { get; set; }
        public DoubleValue TaxableAmount { get; set; }
        public DoubleValue TaxAmount { get; set; }
        public StringValue TaxID { get; set; }
        public DoubleValue TaxRate { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }

}
