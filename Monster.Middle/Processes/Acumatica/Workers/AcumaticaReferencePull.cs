using Monster.Acumatica.Api;
using Monster.Middle.Processes.Acumatica.Persist;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaReferencePull
    {
        private readonly AcumaticaInventoryRepository _dataRepository;
        private readonly ReferenceClient _referenceApi;
        private readonly IPushLogger _logger;

        public AcumaticaReferencePull(
                AcumaticaInventoryRepository dataRepository,
                ReferenceClient referenceApi,
                IPushLogger logger)
        {
            _dataRepository = dataRepository;
            _referenceApi = referenceApi;
            _logger = logger;
        }

        public void RunItemClass()
        {
            var json = _referenceApi.RetrieveItemClass();
            var reference = _dataRepository.RetrieveReferenceData();
            reference.ItemClass = json;
            _dataRepository.SaveChanges();
        }


        public void RunPaymentMethod()
        {
            var json = _referenceApi.RetrievePaymentMethod();
            var reference = _dataRepository.RetrieveReferenceData();
            reference.PaymentMethod = json;
            _dataRepository.SaveChanges();
        }

        public void RunTaxCategories()
        {
            var json = _referenceApi.RetrieveTaxCategories();
            var reference = _dataRepository.RetrieveReferenceData();
            reference.TaxCategory = json;
            _dataRepository.SaveChanges();
        }

        public void RunTaxZones()
        {
            var json = _referenceApi.RetrieveTaxZones();
            var reference = _dataRepository.RetrieveReferenceData();
            reference.TaxZone = json;
            _dataRepository.SaveChanges();
        }

        public void RunTaxIds()
        {
            var json = _referenceApi.RetrieveTaxes();
            var reference = _dataRepository.RetrieveReferenceData();
            reference.TaxId = json;
            _dataRepository.SaveChanges();
        }
    }
}

