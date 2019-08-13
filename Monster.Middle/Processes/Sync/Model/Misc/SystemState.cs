using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Processes.Sync.Model.Misc
{
    public class StateCode
    {
        public const int None = 1;
        public const int Ok = 2;
        public const int Invalid = 3;
        public const int SystemFault = 4;
    }

    public static class SystemStateExtensions
    {
        public static bool IsBroken(this int state)
        {
            return state == StateCode.Invalid ||
                   state == StateCode.SystemFault;
        }

        public static bool IsRealTimeSyncEnabled(this SystemState state)
        {
            return !state.RealTimeHangFireJobId.IsNullOrEmpty();
        }
    }
}

