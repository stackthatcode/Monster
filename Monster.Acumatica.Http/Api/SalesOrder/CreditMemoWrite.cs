using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class CreditMemoWrite
    {
        public StringValue OrderNbr { get; set; }
        public StringValue OrderType { get; set; }
        public StringValue CustomerID { get; set; }

        public List<CreditMemoWriteDetail> Details { get; set; }
        public List<TaxDetails> TaxDetails { get; set; }


        public CreditMemoWrite()
        {
            Details = new List<CreditMemoWriteDetail>();
        }
    }

    public class CreditMemoWriteDetail
    {
        public StringValue WarehouseID;
        public StringValue InventoryID { get; set; }
        public DoubleValue OrderQty { get; set; }
        public StringValue TaxCategory { get; set; }
    }
}

