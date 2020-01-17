using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Misc;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class QueryExtensions
    {
        public static IQueryable<ShopifyOrder> 
                WhereOrderSyncErrorsBelowThreshold(
                    this IQueryable<ShopifyOrder> queryable, 
                    int errorThreshold = SystemConsts.ErrorThreshold)
        {
            return queryable.Where(x => x.PutErrorCount < errorThreshold);
        }

        public static IQueryable<AcumaticaSoShipment> 
                WhereShipmentSyncErrorsBelowThreshold(
                    this IQueryable<AcumaticaSoShipment> queryable, 
                    int errorThreshold = SystemConsts.ErrorThreshold)
        {
            return queryable.Where(x => x.PutErrorCount < errorThreshold);
        }

    }
}
