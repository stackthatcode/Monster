using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Processes.Sync.Inventory.Model
{
    public static class SystemStateValidation
    {
        public static bool IsReadyForRealTimeSync(this UsrSystemState state)
        {
            return state.ShopifyConnState == SystemState.Ok
                   && state.AcumaticaConnState == SystemState.Ok
                   && state.AcumaticaRefDataState == SystemState.Ok
                   && state.PreferenceState == SystemState.Ok
                   && state.WarehouseSyncState == SystemState.Ok;
        }
    }
}
