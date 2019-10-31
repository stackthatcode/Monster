using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Reference;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Model;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Model.Config;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Acumatica.Services
{
    public class ReferenceDataService
    {
        private readonly ShopifyPaymentGatewayService _paymentGatewayService;
        private readonly AcumaticaTimeZoneService _instanceTimeZoneService;
        private readonly AcumaticaInventoryRepository _inventoryRepository;


        public ReferenceDataService(
                ShopifyPaymentGatewayService paymentGatewayService,
                AcumaticaTimeZoneService instanceTimeZoneService,
                AcumaticaInventoryRepository inventoryRepository)
        {
            _paymentGatewayService = paymentGatewayService;
            _instanceTimeZoneService = instanceTimeZoneService;
            _inventoryRepository = inventoryRepository;
        }


        public ReferenceData Retrieve()
        {
            var reference = _inventoryRepository.RetrieveAcumaticaRefeData();

            var timeZones = _instanceTimeZoneService.RetrieveTimeZones();

            var itemClasses =
                reference.ItemClass.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<ItemClass>>()
                    .Where(x => x.DefaultWarehouseID?.value != null)
                    .Select(x => new ItemClassModel(x))
                    .ToList();

            var gateways = _paymentGatewayService.Retrieve();

            var paymentMethods =
                reference.PaymentMethod.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<AcumaticaPaymentMethod>>()
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

            var output = new ReferenceData()
            {
                TimeZones = timeZones,
                ItemClasses = itemClasses,
                PaymentGateways = gateways,
                PaymentMethods = paymentMethods,
                TaxIds = taxIds,
                TaxCategories = taxCategories,
                TaxZones = taxZones,
            };

            return output;
        }


        public void ReconcileSettingsWithRefData(MonsterSetting settings)
        {
            var referenceData = Retrieve();

            if (referenceData
                    .TimeZones
                    .All(x => x.TimeZoneId != settings.AcumaticaTimeZone))
            {
                settings.AcumaticaTimeZone = null;
            }

            if (!referenceData
                    .ItemClasses
                    .Any(x => x.ItemClass == settings.AcumaticaDefaultItemClass
                            && x.PostingClass == settings.AcumaticaDefaultPostingClass))
            {
                settings.AcumaticaDefaultItemClass = null;
                settings.AcumaticaDefaultPostingClass = null;
            }
            
            if (referenceData.TaxCategories.All(x => x != settings.AcumaticaTaxCategory))
            {
                settings.AcumaticaTaxCategory = null;
            }

            if (referenceData.TaxIds.All(x => x != settings.AcumaticaTaxId))
            {
                settings.AcumaticaTaxId = null;
            }

            if (referenceData.TaxZones.All(x => x != settings.AcumaticaTaxZone))
            {
                settings.AcumaticaTaxZone = null;
            }


            if (!referenceData
                .PaymentMethods
                .Any(x => x.PaymentMethod == settings.AcumaticaPaymentMethod
                          && x.CashAccounts.Contains(settings.AcumaticaPaymentCashAccount)))
            {
                settings.AcumaticaPaymentMethod = null;
                settings.AcumaticaPaymentCashAccount = null;
            }


        }

        public List<PaymentGateway> 
                FilterPaymentGatewaysByExistingRefData(List<PaymentGateway> paymentGateways)
        {
            var output = new List<PaymentGateway>();
            var acumaticaPaymentMethods = Retrieve().PaymentMethods;

            foreach (var gateway in paymentGateways)
            {
                var acumaticaPaymentMethod = acumaticaPaymentMethods
                        .FirstOrDefault(x => x.PaymentMethod == gateway.AcumaticaPaymentMethod);

                if (acumaticaPaymentMethod == null)
                {
                    continue;
                }

                var acumaticaCashAccount = 

                output.Add(gateway);
            }

            return output;
        }
    }
}
