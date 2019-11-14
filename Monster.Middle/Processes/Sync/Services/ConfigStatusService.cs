using System.Linq;
using Monster.Middle.Misc.State;
using Monster.Middle.Processes.Acumatica.Services;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Processes.Sync.Services
{
    public class ConfigStatusService
    {
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly StateRepository _stateRepository;
        private readonly CombinedRefDataService _combinedRefDataService;
        private readonly SettingsRepository _settingsRepository;
        

        public ConfigStatusService(
                SyncInventoryRepository syncInventoryRepository, 
                CombinedRefDataService combinedRefDataService,
                StateRepository stateRepository, 
                SettingsRepository settingsRepository)
        {
            _syncInventoryRepository = syncInventoryRepository;
            _stateRepository = stateRepository;
            _settingsRepository = settingsRepository;
            _combinedRefDataService = combinedRefDataService;
        }


        public WarehouseSyncSummary GetWarehouseSyncStatus()
        {
            var warehouses = _syncInventoryRepository.RetrieveWarehouses();
            var locations = _syncInventoryRepository.RetrieveLocations();

            var output = new WarehouseSyncSummary();

            output.UnmatchedAcumaticaWarehouses
                = warehouses
                    .Unmatched()
                    .Select(x => x.AcumaticaWarehouseId)
                    .ToList();

            output.UnmatchedShopifyLocations
                = locations
                    .Unmatched()
                    .Select(x => x.ShopifyLocationName)
                    .ToList();

            output.MatchedWarehouseLocations
                = locations
                    .Matched()
                    .Select(x => x.ShopifyLocationName)
                    .ToList();

            return output;
        }

        public void RefreshWarehouseSyncStatus()
        {
            var status = GetWarehouseSyncStatus();
            var systemState = status.IsOk ? StateCode.Ok : StateCode.Invalid;
            _stateRepository.UpdateSystemState(x => x.WarehouseSyncState, systemState);
        }

        public void RefreshSettingsStatus()
        {
            var settings = _settingsRepository.RetrieveSettings();
            var gateways = _settingsRepository.RetrievePaymentGateways();

            var valid = settings.AcumaticaTimeZone.HasValue()
                        && settings.AcumaticaDefaultItemClass.HasValue()
                        && settings.AcumaticaDefaultPostingClass.HasValue()
                        && gateways.Count > 0;

            var state = valid ? StateCode.Ok : StateCode.Invalid;

            _stateRepository.UpdateSystemState(x => x.SettingsState, state);
        }

        public void RefreshSettingsTaxesStatus()
        {
            var settings = _settingsRepository.RetrieveSettings();

            var valid = settings.AcumaticaTaxZone.HasValue()
                        && settings.AcumaticaTaxableCategory.HasValue()
                        && settings.AcumaticaTaxExemptCategory.HasValue();

            var state = valid ? StateCode.Ok : StateCode.Invalid;

            _stateRepository.UpdateSystemState(x => x.SettingsTaxesState, state);
        }

        public void RefreshStartingShopifyOrderState()
        {
            var settings = _settingsRepository.RetrieveSettings();

            var valid = settings.ShopifyOrderId.HasValue &&
                        settings.ShopifyOrderCreatedAtUtc.HasValue &&
                        settings.ShopifyOrderName.HasValue();

            var state = valid ? StateCode.Ok : StateCode.Invalid;

            _stateRepository.UpdateSystemState(x => x.StartingShopifyOrderState, state);
        }


        public AcumaticaConnectionState GetAcumaticaConnectionStatus()
        {
            var state = _stateRepository.RetrieveSystemStateNoTracking();

            var model = new AcumaticaConnectionState()
            {
                ConnectionState = state.AcumaticaConnState,
                IsUrlFinalized = state.AcumaticaConnState != StateCode.None,
                IsRandomAccessMode = state.IsRandomAccessMode,
            };

            return model;
        }

        public AcumaticaReferenceDataState GetAcumaticaReferenceDataStatus()
        {
            var state = _stateRepository.RetrieveSystemStateNoTracking();
            
            var referenceData = _combinedRefDataService.Retrieve();
            
            var model = new AcumaticaReferenceDataState()
            {
                IsRandomAccessMode = state.IsRandomAccessMode,
                ReferenceDataState = state.AcumaticaRefDataState,
                Validations = referenceData.Validation,
            };

            return model;
        }

        public ConfigStatusSummary GetConfigStatusSummary()
        {
            var state = _stateRepository.RetrieveSystemStateNoTracking();

            var output = new ConfigStatusSummary()
            {
                ShopifyConnection = state.ShopifyConnState,
                AcumaticaConnection = state.AcumaticaConnState,
                AcumaticaReferenceData = state.AcumaticaRefDataState,
                Settings = state.SettingsState,
                SettingsTax = state.SettingsTaxesState,
                WarehouseSync = state.WarehouseSyncState,
                StartingShopifyOrder = state.StartingShopifyOrderState,
            };
            return output;
        }
    }
}

