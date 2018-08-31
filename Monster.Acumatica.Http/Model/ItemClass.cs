using System.Collections.Generic;

namespace Monster.Acumatica.Model
{

    public class ItemClass
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public StringValue AvailabilityCalculationRule { get; set; }
        public StringValue BaseUOM { get; set; }
        public StringValue ClassID { get; set; }
        public StringValue Description { get; set; }
        public StringValue ItemType { get; set; }
        public DateValue LastModifiedDateTime { get; set; }
        public StringValue PostingClass { get; set; }
        public StringValue PriceClass { get; set; }
        public StringValue StockItem { get; set; }
        public StringValue TaxCategoryID { get; set; }
        public StringValue ValuationMethod { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }
}
