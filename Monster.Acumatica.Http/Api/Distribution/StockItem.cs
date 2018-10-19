using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Distribution
{
    
    public class StockItem
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public StringValue AutoIncrementalValue { get; set; }
        public DoubleValue AverageCost { get; set; }
        public StringValue BaseUOM { get; set; }
        public StringValue COGSAccount { get; set; }
        public StringValue Content { get; set; }
        public DoubleValue CurrentStdCost { get; set; }
        public StringValue DefaultIssueLocationID { get; set; }
        public DoubleValue DefaultPrice { get; set; }
        public StringValue DefaultReceiptLocationID { get; set; }
        public StringValue DefaultWarehouseID { get; set; }
        public StringValue Description { get; set; }
        public DoubleValue DimensionVolume { get; set; }
        public DoubleValue DimensionWeight { get; set; }
        public StringValue DiscountAccount { get; set; }
        public StringValue ImageUrl { get; set; }
        public StringValue InventoryAccount { get; set; }
        public StringValue InventoryID { get; set; }
        public StringValue ItemClass { get; set; }
        public StringValue ItemStatus { get; set; }
        public StringValue ItemType { get; set; }
        public StringValue LandedCostVarianceAccount { get; set; }
        public DoubleValue LastCost { get; set; }
        public DateValue LastModified { get; set; }
        public DoubleValue LastStdCost { get; set; }
        public DoubleValue Markup { get; set; }
        public DoubleValue MaxCost { get; set; }
        public DoubleValue MinCost { get; set; }
        public DoubleValue MinMarkup { get; set; }
        public DoubleValue MSRP { get; set; }
        public DoubleValue PendingStdCost { get; set; }
        public StringValue POAccrualAccount { get; set; }
        public StringValue PostingClass { get; set; }
        public StringValue PriceClass { get; set; }
        public StringValue PriceManager { get; set; }
        public StringValue PriceWorkgroup { get; set; }
        public StringValue ProductManager { get; set; }
        public StringValue ProductWorkgroup { get; set; }
        public StringValue PurchasePriceVarianceAccount { get; set; }
        public StringValue SalesAccount { get; set; }
        public StringValue StandardCostRevaluationAccount { get; set; }
        public StringValue StandardCostVarianceAccount { get; set; }
        public BoolValue SubjectToCommission { get; set; }
        public StringValue TaxCategory { get; set; }
        public StringValue ValuationMethod { get; set; }
        public StringValue VolumeUOM { get; set; }
        public StringValue WeightUOM { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }
}
