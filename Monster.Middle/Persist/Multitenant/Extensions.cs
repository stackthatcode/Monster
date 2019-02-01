using System.Data.Entity;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant
{
    public static class Extensions
    {
        public static T ScalarQuery<T>(this DbContext input, string sql)
        {
            return input.Database.SqlQuery<T>(sql).FirstOrDefault();
        }
    }
}
