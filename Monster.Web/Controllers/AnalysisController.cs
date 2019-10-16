using System.Web.Mvc;
using Monster.Middle.Hangfire;
using Monster.Middle.Processes;
using Monster.Middle.Processes.Misc;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Monster.Middle.Processes.Sync.Status;
using Monster.Web.Attributes;
using Monster.Web.Models;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class AnalysisController : Controller
    {
        private readonly StateRepository _stateRepository;
        private readonly ExecutionLogService _logRepository;
        private readonly ExclusiveJobMonitoringService _jobStatusService;
        private readonly ConfigStatusService _statusService;
        private readonly UrlService _urlService;
        private readonly IPushLogger _logger;

        public AnalysisController(
                StateRepository stateRepository,
                OneTimeJobService oneTimeJobService,
                RecurringJobService recurringJobService,
                ExclusiveJobMonitoringService jobStatusService,
                ConfigStatusService statusService,
                ExecutionLogService logRepository,
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
                UrlService urlService,
                IPushLogger logger)
        {
            _stateRepository = stateRepository;
            _jobStatusService = jobStatusService;
            _statusService = statusService;
            _logRepository = logRepository;
            _urlService = urlService;
            _logger = logger;
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
    }
}

