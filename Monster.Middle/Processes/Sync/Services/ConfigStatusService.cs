using System.Linq;
using Monster.Middle.Processes.Acumatica.Services;
using Monster.Middle.Processes.Sync.Model.Config;
using Monster.Middle.Processes.Sync.Model.Extensions;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Persist;

namespace Monster.Middle.Processes.Sync.Services
{
    public class ConfigStatusService
    {
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly SystemStateRepository _stateRepository;
        private readonly ReferenceDataService _referenceDataService;
        

        public ConfigStatusService(
                SyncInventoryRepository syncInventoryRepository, 
                ReferenceDataService referenceDataService,
                SystemStateRepository stateRepository)
        {
            _syncInventoryRepository = syncInventoryRepository;
            _stateRepository = stateRepository;
            _referenceDataService = referenceDataService;
        }


        public WarehouseSyncStateDetails WarehouseSyncStatus()
        {
            var warehouses = _syncInventoryRepository.RetrieveWarehouses();
            var locations = _syncInventoryRepository.RetrieveLocations();

            var output = new WarehouseSyncStateDetails();

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

        public void UpdateWarehouseSyncStatus()
        {
            var status = WarehouseSyncStatus();

            // TODO - maybe guard against transistions from SystemFault state... maybe

            var systemState = status.IsOk ? SystemState.Ok : SystemState.Invalid;

            _stateRepository.UpdateSystemState(x => x.WarehouseSyncState, systemState);

        }


        public AcumaticaConnectionState AcumaticaConnectionStatus()
        {
            var state = _stateRepository.RetrieveSystemState();

            var model = new AcumaticaConnectionState()
            {
                ConnectionState = state.AcumaticaConnState,
                IsUrlFinalized = state.IsAcumaticaUrlFinalized,
                IsRandomAccessMode = state.IsRandomAccessMode,
            };

            return model;
        }

        public AcumaticaReferenceDataState AcumaticaReferenceDataStatus()
        {
            var state = _stateRepository.RetrieveSystemState();
            
            var referenceData = _referenceDataService.Retrieve();
            
            var model = new AcumaticaReferenceDataState()
            {
                IsRandomAccessMode = state.IsRandomAccessMode,
                ReferenceDataState = state.AcumaticaRefDataState,
                Validations = referenceData.Validation,
            };

            return model;
        }


        public SystemStateSummaryModel ConfigSummary()
        {
            var state = _stateRepository.RetrieveSystemState();
            var output = new SystemStateSummaryModel()
            {
                ShopifyConnection = state.ShopifyConnState,
                AcumaticaConnection = state.AcumaticaConnState,
                AcumaticaReferenceData = state.AcumaticaRefDataState,
                InventoryPull = state.InventoryPullState,
                PreferenceSelections = state.PreferenceState,
                WarehouseSync = state.WarehouseSyncState
            };
            return output;
        }
    }
}

