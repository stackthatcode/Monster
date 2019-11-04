using System.Web.Mvc;
using Monster.Middle.Misc.Logging;
using Monster.Web.Attributes;
using Monster.Web.Models;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class AnalysisController : Controller
    {
        private readonly ExecutionLogService _logRepository;

        public AnalysisController(ExecutionLogService logRepository)
        {
            _logRepository = logRepository;
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

        [HttpGet]
        public ActionResult OrderSyncResults()
        {
            return new JsonNetResult(new {});
        }
    }
}

