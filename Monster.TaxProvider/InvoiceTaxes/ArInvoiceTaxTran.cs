namespace Monster.TaxProvider.InvoiceTaxes
{
    public class ArInvoiceTaxTran
    {
        public string InvoiceType { get; set; }
        public string InvoiceNbr { get; set; }
        public string TaxID { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }

        public ArInvoiceTaxTran(PX.Objects.AR.ARTaxTran input)
        {
            TaxID = input.TaxID;
            InvoiceType = input.TranType;
            InvoiceNbr = input.RefNbr;
            TaxableAmount = input.TaxableAmt ?? 0m;
            TaxAmount = input.TaxAmt ?? 0m;
        }
    }
}
