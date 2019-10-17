using System.Linq;
using Monster.Middle.Misc.State;
using Monster.Middle.Processes.Acumatica.Services;
using Monster.Middle.Processes.Sync.Model.Config;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Persist.Matching;

namespace Monster.Middle.Processes.Sync.Status
{
    public class ConfigStatusService
    {
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly StateRepository _stateRepository;
        private readonly ReferenceDataService _referenceDataService;
        

        public ConfigStatusService(
                SyncInventoryRepository syncInventoryRepository, 
                ReferenceDataService referenceDataService,
                StateRepository stateRepository)
        {
            _syncInventoryRepository = syncInventoryRepository;
            _stateRepository = stateRepository;
            _referenceDataService = referenceDataService;
        }


        public WarehouseSyncSummary WarehouseSyncStatus()
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

        public void UpdateWarehouseSyncStatus()
        {
            var status = WarehouseSyncStatus();

            // TODO - maybe guard against transistions from SystemFault state... maybe
            //
            var systemState = status.IsOk ? StateCode.Ok : StateCode.Invalid;
            _stateRepository.UpdateSystemState(x => x.WarehouseSyncState, systemState);
        }


        public AcumaticaConnectionState AcumaticaConnectionStatus()
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

        public AcumaticaReferenceDataState AcumaticaReferenceDataStatus()
        {
            var state = _stateRepository.RetrieveSystemStateNoTracking();
            
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
            var state = _stateRepository.RetrieveSystemStateNoTracking();
            var output = new SystemStateSummaryModel()
            {
                ShopifyConnection = state.ShopifyConnState,
                AcumaticaConnection = state.AcumaticaConnState,
                AcumaticaReferenceData = state.AcumaticaRefDataState,
                InventoryPull = state.InventoryRefreshState,
                PreferenceSelections = state.PreferenceState,
                WarehouseSync = state.WarehouseSyncState
            };
            return output;
        }
    }
}

