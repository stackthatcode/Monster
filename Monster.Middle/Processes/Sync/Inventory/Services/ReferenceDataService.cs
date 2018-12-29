using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Reference;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Sync.Inventory.Services
{
    public class ReferenceDataService
    {
        private readonly TimeZoneService _timeZoneService;
        private readonly AcumaticaInventoryRepository _inventoryRepository;


        public ReferenceDataService(
                TimeZoneService timeZoneService,
                AcumaticaInventoryRepository inventoryRepository)
        {

            _timeZoneService = timeZoneService;
            _inventoryRepository = inventoryRepository;
        }


        public ReferenceData Retrieve()
        {
            var reference = _inventoryRepository.RetrieveReferenceData();

            var itemClasses =
                reference.ItemClass
                    .DeserializeFromJson<List<ItemClass>>()
                    .Select(x => new ItemClassModel(x))
                    .ToList();

            var paymentMethods =
                reference.PaymentMethod
                    .DeserializeFromJson<List<PaymentMethod>>()
                    .Select(x => new PaymentMethodModel(x))
                    .ToList();

            var taxIds =
                reference.TaxId
                    .DeserializeFromJson<List<Tax>>()
                    .Select(x => x.TaxID.value)
                    .ToList();

            var taxCategories =
                reference.TaxCategory
                    .DeserializeFromJson<List<TaxCategory>>()
                    .Select(x => x.TaxCategoryID.value)
                    .ToList();

            var taxZones =
                reference.TaxZone
                    .DeserializeFromJson<List<TaxZone>>()
                    .Select(x => x.TaxZoneID.value)
                    .ToList();

            var timeZones = _timeZoneService.RetrieveTimeZones();

            var output = new ReferenceData()
            {
                ItemClasses = itemClasses,
                PaymentMethods = paymentMethods,
                TaxIds = taxIds,
                TaxCategories = taxCategories,
                TaxZones = taxZones,
                TimeZones = timeZones,
            };

            return output;
        }
    }
}
