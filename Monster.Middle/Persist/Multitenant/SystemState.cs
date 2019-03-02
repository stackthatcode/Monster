namespace Monster.Middle.Persist.Multitenant
{
    public class SystemState
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
            return state == SystemState.Invalid ||
                   state == SystemState.SystemFault;
        }
        
    }
}

