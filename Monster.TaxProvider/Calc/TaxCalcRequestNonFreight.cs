using System.Collections.Generic;


namespace Monster.TaxTransfer
{
    public class TaxCalcRequestNonFreight 
    {
        public List<TaxCalcRequestLineItemDetail> LineItems { get; set; }

        public TaxCalcRequestNonFreight()
        {
            LineItems = new List<TaxCalcRequestLineItemDetail>();
        }
    }

    public class TaxCalcRequestLineItemDetail
    {
        public string InventoryID { get; set; }
        public int Quantity { get; set; }
        public decimal LineAmount { get; set; }
    }
}

