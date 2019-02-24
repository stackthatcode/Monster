﻿using System.Linq;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Inventory.Model;

namespace Monster.Middle.Processes.Sync.Status
{
    public class StatusService
    {
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly StateRepository _stateRepository;
        private readonly OneTimeJobService _hangfireService;
        private readonly ReferenceDataService _referenceDataService;
        
        public StatusService(
                SyncInventoryRepository syncInventoryRepository, 
                ReferenceDataService referenceDataService,
                StateRepository stateRepository,
                OneTimeJobService hangfireService)
        {
            _syncInventoryRepository = syncInventoryRepository;
            _stateRepository = stateRepository;
            _hangfireService = hangfireService;
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

            _stateRepository.UpdateSystemState(x => x.WarehouseSync, systemState);

        }


        public AcumaticaConnectionState AcumaticaConnectionStatus()
        {
            var isRunning =
                _hangfireService.IsJobRunning(
                    BackgroundJobType.ConnectToAcumatica);

            var state = _stateRepository.RetrieveSystemState();

            var model = new AcumaticaConnectionState()
            {
                ConnectionState = state.AcumaticaConnection,
                IsUrlFinalized = state.IsAcumaticaUrlFinalized,
                IsRandomAccessMode = state.IsRandomAccessMode,
                IsBackgroundJobRunning = isRunning,
            };

            return model;
        }

        public AcumaticaReferenceDataState AcumaticaReferenceDataStatus()
        {
            var isRunning =
                _hangfireService.IsJobRunning(
                    BackgroundJobType.PullAcumaticaRefData);

            var state = _stateRepository.RetrieveSystemState();
            
            var referenceData = _referenceDataService.Retrieve();
            
            var model = new AcumaticaReferenceDataState()
            {
                IsRandomAccessMode = state.IsRandomAccessMode,
                ReferenceDataState = state.AcumaticaReferenceData,
                Validations = referenceData.Validation,
                IsBackgroundJobRunning = isRunning,
            };

            return model;
        }
    }
}

