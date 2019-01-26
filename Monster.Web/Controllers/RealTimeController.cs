using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Monster.Middle.Attributes;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Middle.Services;
using Monster.Web.Models;
using Monster.Web.Models.Config;
using Push.Foundation.Web.Json;

namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class RealTimeController : Controller
    {
        private readonly ConnectionRepository _tenantRepository;
        private readonly StateRepository _stateRepository;
        private readonly ExecutionLogRepository _logRepository;
        private readonly HangfireService _hangfireService;

        private readonly StatusService _statusService;
        private readonly ReferenceDataService _referenceDataService;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly AcumaticaBatchRepository _acumaticaBatchRepository;


        public RealTimeController(
                ConnectionRepository tenantRepository,
                StateRepository stateRepository,
                HangfireService hangfireService,
                StatusService statusService, 
                ReferenceDataService referenceDataService, 
                PreferencesRepository preferencesRepository, 
                ExecutionLogRepository logRepository, 
                AcumaticaBatchRepository acumaticaBatchRepository)
        {

            _tenantRepository = tenantRepository;
            _stateRepository = stateRepository;
            _hangfireService = hangfireService;

            _statusService = statusService;
            _referenceDataService = referenceDataService;
            _preferencesRepository = preferencesRepository;
            _logRepository = logRepository;
            _acumaticaBatchRepository = acumaticaBatchRepository;
        }

        

        // Status inquiries
        // 
        [HttpGet]
        public ActionResult RealTime()
        {
            var state = _stateRepository.RetrieveSystemState();
            state.IsRandomAccessMode = true;
            _stateRepository.SaveChanges();

            return View();
        }

        [HttpGet]
        public ActionResult Diagnostics()
        {
            var state = _stateRepository.RetrieveSystemState();
            state.IsRandomAccessMode = true;
            _stateRepository.SaveChanges();

            return View();
        }


        [HttpPost]
        public ActionResult StartRealTime()
        {
            _hangfireService.StartRoutineSync();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult PauseRealTime()
        {
            _hangfireService.PauseRoutineSync();
            return JsonNetResult.Success();
        }
        
        [HttpGet]
        public ActionResult RealTimeStatus()
        {
            var logs = _logRepository.RetrieveExecutionLogs();

            var logDtos =
                logs.Select(x =>
                    new ExecutionLog()
                    {
                        LogTime = x.DateCreated.ToString(),
                        Content = x.LogContent,
                    }).ToList();

            var isConfigDiagnosisRunning 
                = _hangfireService
                    .IsJobRunning(BackgroundJobType.Diagnostics);
            
            var output = new
            {
                IsRealTimeSyncRunning = _hangfireService.IsRealTimeSyncRunning(),
                IsConfigDiagnosisRunning = isConfigDiagnosisRunning,
                Logs = logDtos,
            };
            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult TriggerConfigDiagnosis()
        {
            _hangfireService.TriggerConfigDiagnosis();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult ConfigDiagnosis()
        {
            var state = _stateRepository.RetrieveSystemState();
            var output = Mapper.Map<SystemStateSummaryModel>(state);
            output.IsReadyForRealTimeSync = state.IsReadyForRealTimeSync();

            return new JsonNetResult(output);
        }
    }
}

