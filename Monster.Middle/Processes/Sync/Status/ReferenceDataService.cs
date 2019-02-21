using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Reference;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Sync.Status
{
    public class ReferenceDataService
    {
        private readonly InstanceTimeZoneService _instanceTimeZoneService;
        private readonly AcumaticaInventoryRepository _inventoryRepository;


        public ReferenceDataService(
                InstanceTimeZoneService instanceTimeZoneService,
                AcumaticaInventoryRepository inventoryRepository)
        {

            _instanceTimeZoneService = instanceTimeZoneService;
            _inventoryRepository = inventoryRepository;
        }


        public ReferenceData Retrieve()
        {
            var reference = _inventoryRepository.RetrieveReferenceData();

            var itemClasses =
                reference.ItemClass.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<ItemClass>>()
                    .Where(x => x.DefaultWarehouseID?.value != null)
                    .Select(x => new ItemClassModel(x))
                    .ToList();

            var paymentMethods =
                reference.PaymentMethod.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<PaymentMethod>>()
                    .Select(x => new PaymentMethodModel(x))
                    .ToList();

            var taxIds =
                reference.TaxId.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<Tax>>()
                    .Select(x => x.TaxID.value)
                    .ToList();

            var taxCategories =
                reference.TaxCategory.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<TaxCategory>>()
                    .Select(x => x.TaxCategoryID.value)
                    .ToList();

            var taxZones =
                reference.TaxZone.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<TaxZone>>()
                    .Select(x => x.TaxZoneID.value)
                    .ToList();

            var timeZones = _instanceTimeZoneService.RetrieveTimeZones();

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

        public void FilterPreferencesAgainstRefData(UsrPreference preference)
        {
            var referenceData = Retrieve();

            if (referenceData
                    .TimeZones
                    .All(x => x.TimeZoneId != preference.AcumaticaTimeZone))
            {
                preference.AcumaticaTimeZone = null;
            }

            if (!referenceData
                    .ItemClasses
                    .Any(x => x.ItemClass == preference.AcumaticaDefaultItemClass
                            && x.PostingClass == preference.AcumaticaDefaultPostingClass))
            {
                preference.AcumaticaDefaultItemClass = null;
                preference.AcumaticaDefaultPostingClass = null;
            }
            
            if (!referenceData
                    .PaymentMethods
                    .Any(x => x.PaymentMethod == preference.AcumaticaPaymentMethod
                              && x.CashAccounts.Contains(preference.AcumaticaPaymentCashAccount)))
            {
                preference.AcumaticaPaymentMethod = null;
                preference.AcumaticaPaymentCashAccount = null;
            }

            if (referenceData.TaxCategories.All(x => x != preference.AcumaticaTaxCategory))
            {
                preference.AcumaticaTaxCategory = null;
            }

            if (referenceData.TaxIds.All(x => x != preference.AcumaticaTaxId))
            {
                preference.AcumaticaTaxId = null;
            }

            if (referenceData.TaxZones.All(x => x != preference.AcumaticaTaxZone))
            {
                preference.AcumaticaTaxZone = null;
            }
        }
    }
}
