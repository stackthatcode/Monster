using Monster.TaxProvider.Acumatica;
using Monster.TaxProvider.InvoiceTaxes;
using Monster.TaxProvider.Utility;
using Newtonsoft.Json;


namespace Monster.TaxProvider.InvoiceTaxService
{
    public class OtherInvoiceTaxService
    {
        private readonly BqlRepository _bqlRepository;
        private readonly Logger _logger;

        public OtherInvoiceTaxService(BqlRepository bqlRepository, Logger logger)
        {
            _bqlRepository = bqlRepository;
            _logger = logger;
        }

        public OtherInvoiceTaxContext GetOtherTaxes(string invoiceType, string invoiceRefNbr)
        {
            var salesOrder = _bqlRepository.RetrieveSalesOrderByInvoice(invoiceType, invoiceRefNbr);

            var arTrans =
                _bqlRepository.RetrieveARTaxTransactions(salesOrder.OrderType, salesOrder.OrderNbr);

            var output = new OtherInvoiceTaxContext(arTrans, invoiceType, invoiceRefNbr);

            _logger.Debug($"Other Invoice Taxes - {JsonConvert.SerializeObject(output)}");
            return output;
        }
    }
}

