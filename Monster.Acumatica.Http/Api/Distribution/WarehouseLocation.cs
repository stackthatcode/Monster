using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Distribution
{
    public class WarehouseLocation
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public BoolValue Active { get; set; }
        public BoolValue AssemblyAllowed { get; set; }
        public StringValue Description { get; set; }
        public StringValue LocationID { get; set; }
        public IntegerValue PickPriority { get; set; }
        public BoolValue ReceiptsAllowed { get; set; }
        public BoolValue SalesAllowed { get; set; }
        public BoolValue TransfersAllowed { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }
}
