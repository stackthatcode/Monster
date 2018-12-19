using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Model;
using Monster.Middle.Persist.Sys.Repositories;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Web.Attributes;
using Monster.Web.Models;
using Push.Foundation.Web.Json;
using SystemRepository = Monster.Middle.Persist.Sys.Repositories.SystemRepository;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class ConfigController : Controller
    {
        private readonly TenantRepository _tenantRepository;
        private readonly StateRepository _stateRepository;
        private readonly HangfireService _hangfireService;
        private readonly InventoryStatusService _inventoryStatusService;


        public ConfigController(
                TenantRepository tenantRepository,
                StateRepository stateRepository,
                HangfireService hangfireService, 
                InventoryStatusService inventoryStatusService)
        {
            _tenantRepository = tenantRepository;
            _stateRepository = stateRepository;
            _hangfireService = hangfireService;
            _inventoryStatusService = inventoryStatusService;
        }



        [HttpGet]
        public ActionResult Home()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AcumaticaCreds()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Preferences()
        {
            var preferencesData = _tenantRepository.RetrievePreferences();
            var model = Mapper.Map<Preferences>(preferencesData);
            return View(model);
        }

        [HttpGet]
        public ActionResult Warehouses()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Inventory()
        {
            return View();
        }

        public ActionResult RealTime()
        {
            return View();
        }



        // AJAX methods
        [HttpPost]
        public ActionResult SyncWarehouses()
        {
            _hangfireService.SyncWarehouseAndLocation();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult WarehouseSyncStatus()
        {
            var job = 
                _stateRepository.Retrieve(BackgroundJobType.SyncWarehouseAndLocation);

            var output = new WarehouseSyncStatusModel()
            {
                JobStatus = job.JobStatus
            };

            if (job.JobStatus == JobStatus.Complete)
            {
                output.SyncState
                    = _inventoryStatusService.GetWarehouseSyncStatus();
            }
            
            return new JsonNetResult(output);
        }



        [HttpPost]
        public ActionResult LoadInventoryInAcumatica()
        {
            _hangfireService.PushInventoryToAcumatica();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult LoadInventoryInAcumaticaStatus()
        {
            var job = _stateRepository.Retrieve(BackgroundJobType.PushInventoryToAcumatica);
            var output = new { JobStatus = job.JobStatus };            
            return new JsonNetResult(output);
        }



        [HttpPost]
        public ActionResult LoadInventoryInShopify()
        {
            _hangfireService.PushInventoryToShopify();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult LoadInventoryInShopifyStatus()
        {
            var job = _stateRepository.Retrieve(BackgroundJobType.PushInventoryIntoShopify);
            var output = new { JobStatus = job.JobStatus };
            return new JsonNetResult(output);
        }



        [HttpPost]
        public ActionResult StartRealTime()
        {
            _hangfireService.StartRoutineSync();
            return JsonNetResult.Success();
        }

        public ActionResult RealTimeStatus()
        {
            var logs = _stateRepository.RetrieveExecutionLogs();
            var logDtos = 
                logs.Select(x => 
                    new ExecutionLog()
                    {
                        LogTime = x.DateCreated.ToString(),
                        Content = x.LogContent,
                    }).ToList();

            var output = new
            {
                IsStarted = _hangfireService.IsRoutineSyncStarted(),
                Logs = logDtos,
            };
            return new JsonNetResult(output);
        }

        public ActionResult PauseRealTime()
        {
            _hangfireService.PauseRoutineSync();
            return JsonNetResult.Success();
        }
    }
}

