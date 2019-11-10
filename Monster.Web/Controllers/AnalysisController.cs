using System.Web.Mvc;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
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
        private readonly PendingActionStatusService _orderStatusService;
        private readonly InstanceContext _instanceContext;

        public AnalysisController(
            ExecutionLogService logRepository,
            AnalysisDataService analysisDataService, 
            PendingActionStatusService orderStatusService,
            InstanceContext instanceContext)
        {
            _logRepository = logRepository;
            _analysisDataService = analysisDataService;
            _orderStatusService = orderStatusService;
            _instanceContext = instanceContext;
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

        [HttpGet]
        public ActionResult OrderAnalysis(long shopifyOrderId)
        {
            _instanceContext.InitializeAcumatica(HttpContext.GetIdentity().InstanceId);
            var financialSummary = _analysisDataService.GetOrderFinancialSummary(shopifyOrderId);
            var pendingActionStatus = _orderStatusService.Create(shopifyOrderId);

            return new JsonNetResult(new
            {
                FinancialSummary = financialSummary,
                PendingActionSummary = pendingActionStatus,
            });
        }
    }
}

