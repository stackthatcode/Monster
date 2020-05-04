using Monster.Acumatica.Api;
using Monster.Middle.Processes.Acumatica.Persist;


namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaReferenceGet
    {
        private readonly AcumaticaInventoryRepository _dataRepository;
        private readonly ReferenceClient _referenceApi;

        public AcumaticaReferenceGet(
                AcumaticaInventoryRepository dataRepository, ReferenceClient referenceApi)
        {
            _dataRepository = dataRepository;
            _referenceApi = referenceApi;
        }

        public void RunItemClass()
        {
            var json = _referenceApi.RetrieveItemClass();
            var reference = _dataRepository.RetrieveAcumaticaRefData();
            reference.ItemClass = json;
            _dataRepository.SaveChanges();
        }

        public void RunPaymentMethod()
        {
            var json = _referenceApi.RetrievePaymentMethod();
            var reference = _dataRepository.RetrieveAcumaticaRefData();
            reference.PaymentMethod = json;
            _dataRepository.SaveChanges();
        }

        public void RunTaxCategories()
        {
            var json = _referenceApi.RetrieveTaxCategories();
            var reference = _dataRepository.RetrieveAcumaticaRefData();
            reference.TaxCategory = json;
            _dataRepository.SaveChanges();
        }

        public void RunTaxZones()
        {
            var json = _referenceApi.RetrieveTaxZones();
            var reference = _dataRepository.RetrieveAcumaticaRefData();
            reference.TaxZone = json;
            _dataRepository.SaveChanges();
        }

        public void RunTaxIds()
        {
            var json = _referenceApi.RetrieveTaxes();
            var reference = _dataRepository.RetrieveAcumaticaRefData();
            reference.TaxId = json;
            _dataRepository.SaveChanges();
        }

        public void RunCustomerClasses()
        {
            var json = _referenceApi.RetrieveCustomerClasses();
            var reference = _dataRepository.RetrieveAcumaticaRefData();
            reference.CustomerClass = json;
            _dataRepository.SaveChanges();
        }
    }
}

