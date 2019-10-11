using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;

namespace Monster.TaxProvider.Calc
{
    public class ARTaxTranDigest
    {
        public string InvoiceType { get; set; }
        public string InvoiceNbr { get; set; }
        public string TaxID { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
    }

    public static class ARTaxTranExtensions
    {
        public static ARTaxTranDigest ToDigest(this ARTaxTran input)
        {
            var output = new ARTaxTranDigest();
            output.TaxID = input.TaxID;
            output.InvoiceType = input.TranType;
            output.InvoiceNbr = input.RefNbr;
            output.TaxableAmount = input.TaxableAmt ?? 0m;
            output.TaxAmount = input.TaxAmt ?? 0m;
            return output;
        }

        public static IList<ARTaxTranDigest> ToDigests(this PXResultset<ARTaxTran> input)
        {
            return input.Select(x => ((ARTaxTran)x).ToDigest()).ToList();
        }

        public static IList<ARTaxTranDigest> ExcludeInvoice(
                this IEnumerable<ARTaxTranDigest> input, 
                string invoiceType, 
                string invoiceNbr)
        {
            return input
                .Where(x => x.InvoiceNbr != invoiceNbr && x.InvoiceType != invoiceType)
                .ToList();
        }
    }
}
