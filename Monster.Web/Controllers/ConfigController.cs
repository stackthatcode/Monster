using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Monster.Acumatica.Api.Reference;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Middle.Services;
using Monster.Web.Attributes;
using Monster.Web.Models;
using Monster.Web.Models.Config;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class ConfigController : Controller
    {
        private readonly TenantRepository _tenantRepository;
        private readonly StateRepository _stateRepository;
        private readonly HangfireService _hangfireService;
        private readonly TimeZoneService _timeZoneService;

        private readonly AcumaticaInventoryRepository _inventoryRepository;
        private readonly InventoryStatusService _inventoryStatusService;


        public ConfigController(
                TenantRepository tenantRepository,
                StateRepository stateRepository,
                HangfireService hangfireService,
                TimeZoneService timeZoneService,

                InventoryStatusService inventoryStatusService, 
                AcumaticaInventoryRepository inventoryRepository)
        {

            _tenantRepository = tenantRepository;
            _stateRepository = stateRepository;
            _hangfireService = hangfireService;
            _timeZoneService = timeZoneService;

            _inventoryStatusService = inventoryStatusService;
            _inventoryRepository = inventoryRepository;
        }

        
        // The welcome page
        [HttpGet]
        public ActionResult Splash()
        {
            return View();
        }
        
        // TODO *** Where's this view?
        [HttpGet]
        public ActionResult Home()
        {
            return View();
        }


        // Acumatica Connection stuff...
        //
        [HttpGet]
        public ActionResult AcumaticaInstructions()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AcumaticaConnection()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AcumaticaState()
        {
            var isRunning =
                _hangfireService.IsJobRunning(
                    BackgroundJobType.ConnectToAcumaticaAndPullSettings);

            var state = _stateRepository.RetrieveSystemState();

            var model = new AcumaticaStateModel()
            {
                ConnectionState = state.AcumaticaConnection,
                IsUrlFinalized = state.IsAcumaticaUrlFinalized,
                IsRandomAccessMode = state.IsRandomAccessMode,
                IsBackgroundJobRunning = isRunning,
            };

            return new JsonNetResult(model);
        }

        [HttpGet]
        public ActionResult AcumaticaInstance()
        {
            var tenant = _tenantRepository.Retrieve();

            var model = new AcumaticaCredentialsModel()
            {
                InstanceUrl = tenant.AcumaticaInstanceUrl,
                Branch = tenant.AcumaticaBranch,
                Company = tenant.AcumaticaCompanyName,
            };

            return new JsonNetResult(model);
        }

        [HttpPost]
        public ActionResult AcumaticaInstance(AcumaticaCredentialsModel model)
        {
            var state = _stateRepository.RetrieveSystemState();

            if (state.IsAcumaticaUrlFinalized)
            {
                _tenantRepository
                    .UpdateAcumaticaCredentials(
                        model.UserName,
                        model.Password);
            }
            else
            {
                _tenantRepository
                    .UpdateAcumaticaCredentials(
                        model.InstanceUrl,
                        model.Company,
                        model.Branch,
                        model.UserName,
                        model.Password);
            }

            _hangfireService.ConnectToAcumaticaAndPullSettings();

            return JsonNetResult.Success();
        }


        // Preference-selection of Reference Data
        [HttpGet]
        public ActionResult Preferences()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CurrentPreferences()
        {
            var preferencesData = _tenantRepository.RetrievePreferences();
            var output = Mapper.Map<PreferencesModel>(preferencesData);
            return new JsonNetResult(output);
        }

        [HttpGet]
        public ActionResult ReferenceData()
        {
            var reference = _inventoryRepository.RetrieveReferenceData();

            var itemClasses =
                reference.ItemClass
                    .DeserializeFromJson<List<ItemClass>>()
                    .Select(x => new ItemClassModel(x))
                    .ToList();

            var paymentMethods =
                reference.PaymentMethod
                    .DeserializeFromJson<List<PaymentMethod>>()
                    .Select(x => new PaymentMethodModel(x))
                    .ToList();

            var taxIds =
                reference.TaxId
                    .DeserializeFromJson<List<Tax>>()
                    .Select(x => x.TaxID.value)
                    .ToList();

            var taxCategories =
                reference.TaxCategory
                    .DeserializeFromJson<List<TaxCategory>>()
                    .Select(x => x.TaxCategoryID.value)
                    .ToList();

            var taxZones =
                reference.TaxZone
                    .DeserializeFromJson<List<TaxZone>>()
                    .Select(x => x.TaxZoneID.value)
                    .ToList();

            var timeZones = _timeZoneService.RetrieveTimeZones();
            
            var output = new ReferenceData()
            {
                ItemClasses = itemClasses,
                PaymentMethods = paymentMethods,
                TaxIds = taxIds,
                TaxCategories = taxCategories,
                TaxZones = taxZones,
                TimeZones = timeZones,
            };

            return new JsonNetResult(output);
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

        [HttpGet]
        public ActionResult RealTime()
        {
            return View();
        }



        // Job Launching methods
        //
        [HttpPost]
        public ActionResult SyncWarehouses()
        {
            _hangfireService.SyncWarehouseAndLocation();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult LoadInventoryInAcumatica()
        {
            _hangfireService.PushInventoryToAcumatica();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult LoadInventoryInShopify()
        {
            _hangfireService.PushInventoryToShopify();
            return JsonNetResult.Success();
        }


        // Status inquiries
        // 
        [HttpGet]
        public ActionResult WarehouseSyncStatus()
        {
            var state = _stateRepository.RetrieveSystemState();
            var warehouseSyncStatus
                = _inventoryStatusService.CurrentWarehouseSyncStatus();

            var output = new WarehouseSyncStatusModel()
            {
                JobStatus = state.WarehouseSync,
                SyncState = warehouseSyncStatus
            };

            return new JsonNetResult(output);
        }

        [HttpGet]
        public ActionResult AcumaticaInventoryPushStatus()
        {
            var state = _stateRepository.RetrieveSystemState();
            var output = new { JobStatus = state.AcumaticaInventoryPush };
            return new JsonNetResult(output);
        }

        [HttpGet]
        public ActionResult ShopifyInventoryPushStatus()
        {
            var state = _stateRepository.RetrieveSystemState();
            var output = new { JobStatus = state.ShopifyInventoryPush };
            return new JsonNetResult(output);
        }


        // Real Time Sync
        //
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

