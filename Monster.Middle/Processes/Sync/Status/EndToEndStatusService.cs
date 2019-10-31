using Monster.Middle.Misc.State;
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
            var configStatus = _configStatusService.GetConfigStatusSummary();
            var state = _stateRepository.RetrieveSystemStateNoTracking();

            var output = new EndToEndSyncStatus();
            output.ConfigStatusSummaryModel = configStatus;
            output.ShopifyOrderState = state.StartingShopifyOrderState;

            return output;
        }

    }
}
