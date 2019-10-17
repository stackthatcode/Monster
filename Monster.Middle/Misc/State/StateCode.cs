namespace Monster.Middle.Misc.State
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
    }
}

