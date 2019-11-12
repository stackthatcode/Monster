using System.Web.Mvc;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.State;
using Monster.Web.Attributes;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class JobController : Controller
    {
        private readonly ExecutionLogService _logRepository;
        private readonly JobMonitoringService _jobStatusService;
        private readonly StateRepository _stateRepository;

        public JobController(
                JobMonitoringService jobStatusService,
                ExecutionLogService logRepository, 
                StateRepository stateRepository)
        {
            _jobStatusService = jobStatusService;
            _logRepository = logRepository;
            _stateRepository = stateRepository;
        }


        // Status inquiries
        // 
        [HttpGet]
        public ActionResult Status()
        {
            var output = new
            {
                IsEndToEndSyncRunning = _jobStatusService.IsJobRunning(BackgroundJobType.EndToEndSync),
                AreAnyJobsRunning = _jobStatusService.AreAnyJobsRunning(),
                Logs = _logRepository.RetrieveExecutionLogs(100),
            };
            return new JsonNetResult(output);
        }

        [HttpGet]
        public ActionResult InventoryRefreshStatus()
        {
            var state = _stateRepository.RetrieveSystemState();
            var output = new { state.InventoryRefreshState };
            return new JsonNetResult(output);
        }
    }
}
