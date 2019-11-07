using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class SalesOrderUpdate
    {
        public StringValue OrderType { get; set; }
        public StringValue OrderNbr  { get; set; }
        public BoolValue Hold { get; set; }

        public List<SalesOrderUpdateDetail> Details { get; set; }
        public BoolValue OverrideFreightPrice { get; set; }
        public DoubleValue FreightPrice { get; set; }
        public SalesOrderUsrTaxSnapshot custom;

        public SalesOrderUpdate()
        {
            Details = new List<SalesOrderUpdateDetail>();
        }
    }

    public class SalesOrderUpdateDetail
    {
        public string id { get; set; }
        public StringValue InventoryID { get; set; }
        public DoubleValue OrderQty { get; set; }
        //public bool delete => true;
    }
}
