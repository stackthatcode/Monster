using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class SalesOrderWrite
    {
        public StringValue OrderNbr  { get; set; }
        public StringValue OrderType { get; set; }
        public List<SalesOrderUpdateDetail> Details { get; set; }

        public SalesOrderWrite()
        {
            Details = new List<SalesOrderUpdateDetail>();
        }
    }

    public class SalesOrderUpdateDetail
    {
        public string id { get; set; }
        public DoubleValue Quantity { get; set; }
    }
}
