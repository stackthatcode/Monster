using System.Collections.Generic;
using System.Data.Entity;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Analysis;
using Monster.Web.Models.Analysis;

namespace Monster.Middle.Processes.Sync.Services
{
    public class AnalysisDataService
    {
        private readonly ProcessPersistContext _persistContext;

        public AnalysisDataService(ProcessPersistContext persistContext)
        {
            _persistContext = persistContext;
        }

        public List<OrderAnalyzerGridRow> GetOrderAnalysis(OrderAnalyzerRequest request)
        {
            var output = new List<OrderAnalyzerGridRow>();

            //var queryable = _persistContext
            //    .Entities
            //    .ShopifyOrders
            //    .Include(x => x.ShopAcuOrderSyncs)



            return output;
        }
    }
}
