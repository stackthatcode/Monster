using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class ReturnForCreditWrite
    {
        public StringValue OrderNbr { get; set; }
        public StringValue OrderType { get; set; }
        public StringValue CustomerID { get; set; }
        public StringValue Description { get; set; }

        public List<ReturnForCreditWriteDetail> Details { get; set; }
        public List<TaxDetails> TaxDetails { get; set; }


        public ReturnForCreditWrite()
        {
            Details = new List<ReturnForCreditWriteDetail>();
        }
    }

    public class ReturnForCreditWriteDetail
    {
        public StringValue WarehouseID;
        public StringValue InventoryID { get; set; }
        public DoubleValue OrderQty { get; set; }
        public StringValue TaxCategory { get; set; }
    }
}

