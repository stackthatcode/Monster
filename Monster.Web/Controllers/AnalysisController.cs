using System.Web.Mvc;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Processes.Sync.Model.Analysis;
using Monster.Middle.Processes.Sync.Services;
using Monster.Web.Attributes;
using Monster.Web.Models;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class AnalysisController : Controller
    {
        private readonly ExecutionLogService _logRepository;
        private readonly AnalysisDataService _analysisDataService;

        public AnalysisController(
            ExecutionLogService logRepository,
            AnalysisDataService analysisDataService)
        {
            _logRepository = logRepository;
            _analysisDataService = analysisDataService;
        }


        [HttpGet]
        public ActionResult ExecutionLogs()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ExecutionLogData()
        {
            var logs = _logRepository.RetrieveExecutionLogs().ToModel();
            return new JsonNetResult(logs);
        }

        [HttpGet]
        public ActionResult OrderSync()
        {
            return View();
        }

        [HttpPost]
        public ActionResult OrderSyncResults(OrderAnalyzerRequest request)
        {
            var grid = _analysisDataService.GetOrderAnalysisResults(request);
            var count = _analysisDataService.GetOrderAnalysisRecordCount(request);

            return new JsonNetResult(new {Grid = grid, Count = count});
        }

        [HttpPost]
        public ActionResult OrderSyncDrilldown(long shopifyOrderId)
        {

            return new JsonNetResult(new {});
        }
    }
}

