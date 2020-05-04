using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Reference;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Persist.Instance;
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
        private readonly ReferenceDataRepository _referenceDataRepository;
        private readonly SettingsRepository _settingsRepository;
        private readonly ExecutionLogService _logService;

        public CombinedRefDataService(
                ShopifyPaymentGatewayService paymentGatewayService,
                AcumaticaTimeZoneService instanceTimeZoneService,
                SettingsRepository settingsRepository, 
                ReferenceDataRepository referenceDataRepository,
                ExecutionLogService logService)
        {
            _paymentGatewayService = paymentGatewayService;
            _instanceTimeZoneService = instanceTimeZoneService;
            _settingsRepository = settingsRepository;
            _referenceDataRepository = referenceDataRepository;
            _logService = logService;
        }

        public CombinedReferenceData RetrieveRefData()
        {
            var reference = _referenceDataRepository.RetrieveAcumaticaRefData();

            var timeZones = _instanceTimeZoneService.RetrieveTimeZones();
            var gateways = _paymentGatewayService.Retrieve();

            var itemClasses =
                reference.AcumaticaItemClass.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<ItemClass>>()
                    .Where(x => x.DefaultWarehouseID?.value != null)
                    .Select(x => new ItemClassModel(x))
                    .ToList();

            var paymentMethods =
                reference.AcumaticaPaymentMethod.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<AcumaticaPaymentMethod>>()
                    .Select(x => new PaymentMethodModel(x))
                    .Where(x => x.Validation.Success)
                    .ToList();

            var taxIds =
                reference.AcumaticaTaxId.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<Tax>>()
                    .Select(x => x.TaxID.value)
                    .ToList();

            var taxCategories =
                reference.AcumaticaTaxCategory.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<TaxCategory>>()
                    .Select(x => x.TaxCategoryID.value)
                    .ToList();

            var taxZones =
                reference.AcumaticaTaxZone.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<TaxZone>>()
                    .Select(x => x.TaxZoneID.value)
                    .ToList();

            var customerClasses =
                reference.AcumaticaCustomerClass.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<CustomerClass>>()
                    .Select(x => x.ClassID.value)
                    .ToList();

            var carriers =
                reference.ShopifyCarrier.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<ShopifyCarrierParent>()
                    .carrier_services
                    .Select(x => x.name)
                    .ToList();

            var shipVia =
                reference.AcumaticaShipVia.IsNullOrEmptyAlt("[]")
                    .DeserializeFromJson<List<AcumaticaShipVia>>()
                    .Select(x => x.CarrierID.value)
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
                CustomerClasses = customerClasses,
                AcumaticaShipVia = shipVia,
                ShopifyCarriers = carriers,
            };

            return output;
        }

        public void ReconcileSettingsWithRefData()
        {
            var referenceData = RetrieveRefData();
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
            
            if (settings.AcumaticaTaxableCategory != null &&
                referenceData.TaxCategories.All(x => x != settings.AcumaticaTaxableCategory))
            {
                _logService.Log($"Tax Category {settings.AcumaticaTaxableCategory} is missing from Acumatica");
                settings.AcumaticaTaxableCategory = null;
            }

            if (settings.AcumaticaTaxExemptCategory != null &&
                referenceData.TaxCategories.All(x => x != settings.AcumaticaTaxExemptCategory))
            {
                _logService.Log($"Tax Category {settings.AcumaticaTaxExemptCategory} is missing from Acumatica");
                settings.AcumaticaTaxExemptCategory = null;
            }

            if (settings.AcumaticaTaxZone != null &&
                referenceData.TaxZones.All(x => x != settings.AcumaticaTaxZone))
            {
                _logService.Log($"Tax Zone {settings.AcumaticaTaxZone} is missing from Acumatica");
                settings.AcumaticaTaxZone = null;
            }

            if (settings.AcumaticaCustomerClass != null &&
                referenceData.CustomerClasses.All(x => x != settings.AcumaticaCustomerClass))
            {
                _logService.Log($"Customer Class {settings.AcumaticaCustomerClass} is missing from Acumatica");
                settings.AcumaticaCustomerClass = null;
            }

            _settingsRepository.SaveChanges();
        }

        public void ReconcilePaymentGatewaysWithRefData()
        {
            var referenceData = RetrieveRefData();
            var settingsGateways = _settingsRepository.RetrievePaymentGateways();

            var deleteList = new List<PaymentGateway>();

            foreach (var settingsGateway in settingsGateways)
            {
                var selectedShopifyGatewayId = settingsGateway.ShopifyGatewayId;
                var selectedPaymentMethod = settingsGateway.AcumaticaPaymentMethod;
                
                // Remove if Payment Gateway is no longer supported by Bridge
                //
                if (!referenceData.PaymentGateways.Any(x => x.Id == selectedShopifyGatewayId))
                {
                    _logService.Log($"Payment Gateway {selectedShopifyGatewayId} is missing");
                    deleteList.Add(settingsGateway);
                    continue;
                }

                // Remove if Payment Method is missing from Acumatica pull
                //
                var acumaticaPaymentMethod 
                        = referenceData
                            .PaymentMethods
                            .FirstOrDefault(x => x.PaymentMethod == selectedPaymentMethod);

                if (acumaticaPaymentMethod == null)
                {
                    _logService.Log($"Payment Method {selectedPaymentMethod} is missing");
                    deleteList.Add(settingsGateway);
                    continue;
                }

                // Remove if Acumatica Payment method is invalid
                //
                if (!acumaticaPaymentMethod.Validation.Success)
                {
                    _logService.Log($"Payment Method {selectedPaymentMethod} is invalid");
                    deleteList.Add(settingsGateway);
                }
            }

            foreach (var gateway in deleteList)
            {
                _settingsRepository.DeletePaymentGateway(gateway.Id);
            }
        }

    }
}
