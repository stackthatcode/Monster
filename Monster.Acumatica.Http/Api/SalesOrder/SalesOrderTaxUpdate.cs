using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.SalesOrder
{
    public class SalesOrderTaxUpdate
    {
        public StringValue OrderNbr  { get; set; }
        public StringValue OrderType { get; set; }
        public List<TaxDetails> TaxDetails { get; set; }
        public BoolValue Hold { get; set; }

    }    
}
