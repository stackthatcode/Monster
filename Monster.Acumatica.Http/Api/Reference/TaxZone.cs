using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Reference
{
    public class TaxZone
    {
        public DateValue CreatedDateTime { get; set; }
        public StringValue Description { get; set; }
        public DateValue LastModifiedDateTime { get; set; }
        public StringValue TaxZoneID { get; set; }
        public string custom { get; set; }
        public string id { get; set; }
        public string note { get; set; }
        public int rowNumber { get; set; }        
    }
}
