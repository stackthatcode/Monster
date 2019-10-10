using System;
using System.Collections.Generic;
using System.Linq;

namespace Monster.TaxTransfer
{
    public class TransferLineItem
    {
        public string ExternalRefNbr { get; set; }
        public string InventoryID { get; set; }
        public bool IsTaxable { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public List<TransferTaxLine> TaxLines { get; set; }

        public TransferLineItem()
        {
            TaxLines = new List<TransferTaxLine>();
        }

        public decimal TotalPrice => Quantity * UnitPrice;
        public decimal TaxableAmount => IsTaxable ? TotalPrice : 0m;
        public decimal TotalTax => IsTaxable ? TaxLines.CalculateTaxes(TaxableAmount) : 0m;

        public decimal CalcSplitShipmentTaxes(int shippedQty)
        {
            return TaxLines.CalculateTaxes(shippedQty * UnitPrice);
        }
    }
}
