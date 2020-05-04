using Monster.Acumatica.Api;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;


namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaReferenceGet
    {
        private readonly ReferenceDataRepository _dataRepository;
        private readonly ReferenceClient _referenceApi;

        public AcumaticaReferenceGet(ReferenceDataRepository dataRepository, ReferenceClient referenceApi)
        {
            _dataRepository = dataRepository;
            _referenceApi = referenceApi;
        }


        public void RunItemClass()
        {
            var json = _referenceApi.RetrieveItemClass();
            var reference = _dataRepository.RetrieveAcumaticaRefData();
            reference.AcumaticaItemClass = json;
            _dataRepository.SaveChanges();
        }

        public void RunPaymentMethod()
        {
            var json = _referenceApi.RetrievePaymentMethod();
            var reference = _dataRepository.RetrieveAcumaticaRefData();
            reference.AcumaticaPaymentMethod = json;
            _dataRepository.SaveChanges();
        }

        public void RunTaxCategories()
        {
            var json = _referenceApi.RetrieveTaxCategories();
            var reference = _dataRepository.RetrieveAcumaticaRefData();
            reference.AcumaticaTaxCategory = json;
            _dataRepository.SaveChanges();
        }

        public void RunTaxZones()
        {
            var json = _referenceApi.RetrieveTaxZones();
            var reference = _dataRepository.RetrieveAcumaticaRefData();
            reference.AcumaticaTaxZone = json;
            _dataRepository.SaveChanges();
        }

        public void RunTaxIds()
        {
            var json = _referenceApi.RetrieveTaxes();
            var reference = _dataRepository.RetrieveAcumaticaRefData();
            reference.AcumaticaTaxId = json;
            _dataRepository.SaveChanges();
        }

        public void RunCustomerClasses()
        {
            var json = _referenceApi.RetrieveCustomerClasses();
            var reference = _dataRepository.RetrieveAcumaticaRefData();
            reference.AcumaticaCustomerClass = json;
            _dataRepository.SaveChanges();
        }

        public void RunShipVia()
        {
            var json = _referenceApi.RetrieveShipVia();
            var reference = _dataRepository.RetrieveAcumaticaRefData();
            reference.AcumaticaShipVia = json;
            _dataRepository.SaveChanges();
        }
    }
}

