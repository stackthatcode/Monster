using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Reference;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Model.Reference;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Acumatica.Services
{
    public class CombinedRefDataService
    {
        private readonly ShopifyPaymentGatewayService _paymentGatewayService;
        private readonly AcumaticaTimeZoneService _instanceTimeZoneService;
        private readonly AcumaticaInventoryRepository _inventoryRepository;
        private readonly SettingsRepository _settingsRepository;

        public CombinedRefDataService(
                ShopifyPaymentGatewayService paymentGatewayService,
                AcumaticaTimeZoneService instanceTimeZoneService,
                AcumaticaInventoryRepository inventoryRepository, 
                SettingsRepository settingsRepository)
        {
            _paymentGatewayService = paymentGatewayService;
            _instanceTimeZoneService = instanceTimeZoneService;
            _inventoryRepository = inventoryRepository;
            _settingsRepository = settingsRepository;
        }

        public CombinedReferenceData Retrieve()
        {
            var reference = _inventoryRepository.RetrieveAcumaticaRefData();

            var timeZones = _instanceTimeZoneService.RetrieveTimeZones();
            var gateways = _paymentGatewayService.Retrieve();

            var itemClasses =
                reference.ItemClass.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<ItemClass>>()
                    .Where(x => x.DefaultWarehouseID?.value != null)
                    .Select(x => new ItemClassModel(x))
                    .ToList();

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

            var output = new CombinedReferenceData()
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

        public void ReconcileSettingsWithRefData()
        {
            var referenceData = Retrieve();
            var settings = _settingsRepository.RetrieveSettings();
            
            if (referenceData.TimeZones.All(x => x.TimeZoneId != settings.AcumaticaTimeZone))
            {
                settings.AcumaticaTimeZone = null;
            }

            if (!referenceData
                    .ItemClasses.Any(
                        x => x.ItemClass == settings.AcumaticaDefaultItemClass
                            && x.PostingClass == settings.AcumaticaDefaultPostingClass))
            {
                settings.AcumaticaDefaultItemClass = null;
                settings.AcumaticaDefaultPostingClass = null;
            }
            
            if (referenceData.TaxCategories.All(x => x != settings.AcumaticaTaxableCategory))
            {
                settings.AcumaticaTaxableCategory = null;
            }

            if (referenceData.TaxCategories.All(x => x != settings.AcumaticaTaxExemptCategory))
            {
                settings.AcumaticaTaxExemptCategory = null;
            }

            if (referenceData.TaxIds.All(x => x != settings.AcumaticaLineItemTaxId))
            {
                settings.AcumaticaLineItemTaxId = null;
            }

            if (referenceData.TaxIds.All(x => x != settings.AcumaticaFreightTaxId))
            {
                settings.AcumaticaFreightTaxId = null;
            }

            if (referenceData.TaxZones.All(x => x != settings.AcumaticaTaxZone))
            {
                settings.AcumaticaTaxZone = null;
            }

            _settingsRepository.SaveChanges();
        }

        public void ReconcilePaymentGatewaysWithRefData()
        {
            var referenceData = Retrieve();
            var gatewaySettings = _settingsRepository.RetrievePaymentGateways();

            var deleteList = new List<Middle.Persist.Instance.PaymentGateway>();

            foreach (var gateway in gatewaySettings)
            {
                var selectedShopifyGatewayId = gateway.ShopifyGatewayId;
                var selectedPaymentMethod = gateway.AcumaticaPaymentMethod;
                var selectedCashAccount = gateway.AcumaticaCashAccount;

                if (!referenceData.PaymentGateways.Any(x => x.Id == selectedShopifyGatewayId))
                {
                    deleteList.Add(gateway);
                    continue;
                }

                var acumaticaPaymentMethod 
                    = referenceData.PaymentMethods
                            .FirstOrDefault(x => x.PaymentMethod == selectedPaymentMethod);

                if (acumaticaPaymentMethod == null)
                {
                    deleteList.Add(gateway);
                    continue;
                }

                if (!acumaticaPaymentMethod.CashAccounts.Contains(selectedCashAccount))
                {
                    deleteList.Add(gateway);
                }
            }

            foreach (var gateway in deleteList)
            {
                _settingsRepository.DeletePaymentGateway(gateway.Id);
            }
        }

    }
}
