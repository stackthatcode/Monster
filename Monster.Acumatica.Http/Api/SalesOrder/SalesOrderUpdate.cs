using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class SalesOrderUpdate
    {
        public StringValue OrderNbr  { get; set; }
        public StringValue OrderType { get; set; }
        public BoolValue Hold { get; set; }

        public List<SalesOrderUpdateDetail> Details { get; set; }
        public SalesOrderTotals Totals { get; set; }
        public SalesOrderUsrTaxSnapshot custom;


        public SalesOrderUpdate()
        {
            Details = new List<SalesOrderUpdateDetail>();
        }
    }

    public class SalesOrderUpdateDetail
    {
        public string id { get; set; }
        //public StringValue InventoryID { get; set; }
        public DoubleValue Quantity { get; set; }
    }
}
