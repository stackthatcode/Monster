using System.Web.Mvc;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.State;
using Monster.Web.Attributes;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    [Authorize(Roles = "ADMIN, USER")]
    public class JobController : Controller
    {
        private readonly ExecutionLogService _logRepository;
        private readonly JobMonitoringService _jobStatusService;
        private readonly RecurringJobScheduler _recurringJobScheduler;
        private readonly StateRepository _stateRepository;

        public JobController(
                JobMonitoringService jobStatusService,
                ExecutionLogService logRepository, 
                StateRepository stateRepository, 
                RecurringJobScheduler recurringJobScheduler)
        {
            _jobStatusService = jobStatusService;
            _logRepository = logRepository;
            _stateRepository = stateRepository;
            _recurringJobScheduler = recurringJobScheduler;
        }


        // Status inquiries
        // 
        [HttpGet]
        public ActionResult Status()
        {
            var areAnyJobsRunning = _jobStatusService.AreAnyJobsRunning();
            var logs = _logRepository.RetrieveExecutionLogs(100);

            var output = new
            {
                AreAnyJobsRunning = areAnyJobsRunning,
                Logs = logs
            };
            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult StopAll()
        {
            _jobStatusService.SendKillSignal();
            return JsonNetResult.Success();
        }


        [HttpGet]
        public ActionResult InventoryRefreshStatus()
        {
            var state = _stateRepository.RetrieveSystemState();
            var output = new { state.InventoryRefreshState };
            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult Cleanup()
        {
            _jobStatusService.Cleanup();
            return JsonNetResult.Success();
        }

    }
}
