﻿using Monster.Middle.Misc.State;
using Monster.Middle.Processes.Sync.Model.Status;

namespace Monster.Middle.Processes.Sync.Status
{
    public class EndToEndStatusService
    {
        private readonly ConfigStatusService _configStatusService;
        private readonly StateRepository _stateRepository;

        public EndToEndStatusService(
                ConfigStatusService configStatusService, 
                StateRepository stateRepository)
        {
            _configStatusService = configStatusService;
            _stateRepository = stateRepository;
        }

        public EndToEndSyncStatus GetEndToEndSyncStatus()
        {
            var configStatus = _configStatusService.GetConfigSummary();
            var state = _stateRepository.RetrieveSystemStateNoTracking();

            var output = new EndToEndSyncStatus();
            output.ConfigStateSummaryModel = configStatus;
            output.StartingShopifyOrderState = state.StartingShopifyOrderState;

            return output;
        }
    }
}