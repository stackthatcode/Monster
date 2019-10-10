using System;
using System.Collections.Generic;
using System.Linq;

namespace Monster.TaxTransfer
{
    public class TransferRefund
    {
        public string ExternalRefNbr { get; set; }
        public decimal RefundAmount { get; set; }

        public decimal TotalTaxableLineAmounts { get; set; }
        public decimal TotalLineItemsTax { get; set; }
        
        public decimal TaxableFreightAmount { get; set; }
        public decimal FreightTax { get; set; }
    }
}

