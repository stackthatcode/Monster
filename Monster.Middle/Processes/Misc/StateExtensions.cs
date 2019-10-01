using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Misc
{
    public static class StateExtensions
    {
        public static T ScalarQuery<T>(this DbContext input, string sql)
        {
            return input.Database.SqlQuery<T>(sql).FirstOrDefault();
        }

        public static bool IsShopifyUrlFinalized(this SystemState state)
        {
            return state.ShopifyConnState != StateCode.None;
        }

        public static bool IsAcumaticaUrlFinalized(this SystemState state)
        {
            return state.AcumaticaConnState != StateCode.None;
        }
    }
}
