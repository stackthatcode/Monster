using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Reference
{
    public class TaxCategory
    {
        public BoolValue Active { get; set; }
        public DateValue CreatedDateTime { get; set; }
        public StringValue Description { get; set; }
        public BoolValue ExcludeListedTaxes { get; set; }
        public DateValue LastModifiedDateTime { get; set; }
        public StringValue TaxCategoryID { get; set; }
        public string custom { get; set; }
        public string id { get; set; }
        public string note { get; set; }
        public int rowNumber { get; set; }
    }
}
