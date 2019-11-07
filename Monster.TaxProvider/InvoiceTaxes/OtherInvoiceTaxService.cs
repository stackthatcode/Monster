using Monster.TaxProvider.Bql;
using Monster.TaxProvider.InvoiceTaxes;


namespace Monster.TaxProvider.InvoiceTaxService
{
    public class OtherInvoiceTaxService
    {
        private readonly BqlRepository _bqlRepository;

        public OtherInvoiceTaxService(BqlRepository bqlRepository)
        {
            this._bqlRepository = bqlRepository;
        }

        public OtherInvoiceTaxSummary 
                    GetOtherTaxesSummary(string invoiceType, string invoiceRefNbr, string taxId)
        {
            var salesOrder = _bqlRepository.RetrieveSalesOrderByInvoice(invoiceType, invoiceRefNbr);

            var arTrans =
                _bqlRepository.RetrieveARTaxTransactions(salesOrder.OrderType, salesOrder.OrderNbr, taxId);

            return new OtherInvoiceTaxSummary(arTrans, invoiceType, invoiceRefNbr);
        }

    }
}

