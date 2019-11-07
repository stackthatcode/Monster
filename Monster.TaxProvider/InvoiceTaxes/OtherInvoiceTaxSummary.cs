using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AR;


namespace Monster.TaxProvider.InvoiceTaxes
{
    public class OtherInvoiceTaxSummary
    {
        public List<ArInvoiceTaxTran> Items { get; private set; }

        public OtherInvoiceTaxSummary(
                    PXResultset<ARTaxTran> input, string exclInvoiceType, string exclInvoiceNbr)
        {
            Items = input
                .Select(x => new ArInvoiceTaxTran((ARTaxTran)x))
                .Where(x => x.InvoiceNbr != exclInvoiceNbr && x.InvoiceType != exclInvoiceType)
                .ToList();
        }
    }
}
