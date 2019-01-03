using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Processes.Sync.Inventory.Model
{
    public static class SystemStateValidation
    {
        public static bool IsReadyForRealTimeSync(this UsrSystemState state)
        {
            return state.ShopifyConnection == SystemState.Ok
                   && state.AcumaticaConnection == SystemState.Ok
                   && state.AcumaticaReferenceData == SystemState.Ok
                   && state.PreferenceSelections == SystemState.Ok
                   && state.WarehouseSync == SystemState.Ok
                   && state.ShopifyInventoryPush == SystemState.Ok;
        }
    }
}
