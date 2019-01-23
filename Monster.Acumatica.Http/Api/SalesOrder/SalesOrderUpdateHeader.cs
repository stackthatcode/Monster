using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class SalesOrderUpdateHeader
    {
        public StringValue OrderNbr  { get; set; }
        public StringValue OrderType { get; set; }
        public List<SalesOrderUpdateDetail> Details { get; set; }
        public BoolValue Hold { get; set; }

        public SalesOrderUpdateHeader()
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
