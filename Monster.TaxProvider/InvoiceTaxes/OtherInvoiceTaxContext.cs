using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;


namespace Monster.TaxProvider.InvoiceTaxes
{
    public class OtherInvoiceTaxContext
    {
        private List<ArInvoiceTaxTran> Items { get; set; }
        public string ExcludedInvoiceType { get; set; }
        public string ExcludedInvoiceNbr { get; set; }

        private List<ArInvoiceTaxTran> FilteredItems =>
            Items.Where(x => x.InvoiceNbr != ExcludedInvoiceType && x.InvoiceType != ExcludedInvoiceNbr)
                .ToList();


        public decimal TotalTaxableAmount => Items.Sum(x => x.TaxableAmount);
        public decimal TotalTaxAmount => Items.Sum(x => x.TaxAmount);

        public OtherInvoiceTaxContext(
                    PXResultset<ARTaxTran> input, string exclInvoiceType, string exclInvoiceNbr)
        {
            ExcludedInvoiceType = exclInvoiceType;
            ExcludedInvoiceNbr = exclInvoiceNbr;

            Items = input
                .Select(x => new ArInvoiceTaxTran((ARTaxTran)x))
                .ToList();
        }
    }
}
