using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Monster.Middle.Attributes;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Middle.Services;
using Monster.Web.Models;
using Monster.Web.Models.Config;
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
        private readonly StatusService _statusService;
        private readonly ReferenceDataService _referenceDataService;


        public ConfigController(
                TenantRepository tenantRepository,
                StateRepository stateRepository,
                HangfireService hangfireService,
                TimeZoneService timeZoneService,
                StatusService statusService, 
                ReferenceDataService referenceDataService, 
                AcumaticaInventoryRepository inventoryRepository)
        {

            _tenantRepository = tenantRepository;
            _stateRepository = stateRepository;
            _hangfireService = hangfireService;
            _timeZoneService = timeZoneService;

            _statusService = statusService;
            _referenceDataService = referenceDataService;
            _inventoryRepository = inventoryRepository;
        }

        
        // The welcome page
        [HttpGet]
        public ActionResult Splash()
        {
            return View();
        }
        


        // Acumatica Connection (credentials stuff...)
        //        
        [HttpGet]
        public ActionResult AcumaticaConnection()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AcumaticaConnectionStatus()
        {
            var model = _statusService.AcumaticaConnectionStatus();
            return new JsonNetResult(model);
        }

        [HttpGet]
        public ActionResult AcumaticaCredentials()
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
        public ActionResult AcumaticaTestConnection()
        {
            _hangfireService.ConnectToAcumatica();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult AcumaticaCredentials(AcumaticaCredentialsModel model)
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

            _hangfireService.ConnectToAcumatica();
            return JsonNetResult.Success();
        }



        // Acumatica Settings Pull
        //
        [HttpGet]
        public ActionResult AcumaticaRefData()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AcumaticaRefDataPull()
        {
            _hangfireService.PullAcumaticaReferenceData();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult AcumaticaRefDataStatus()
        {
            var status = _statusService.AcumaticaReferenceDataStatus();
            return new JsonNetResult(status);
        }



        // Preference-selection of Reference Data
        //
        [HttpGet]
        public ActionResult Preferences()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AcumaticaReferenceData()
        {
            var data = _referenceDataService.Retrieve();
            return new JsonNetResult(data);
        }

        [HttpGet]
        public ActionResult PreferenceSelections()
        {
            var preferencesData = _tenantRepository.RetrievePreferences();
            var output = Mapper.Map<PreferencesModel>(preferencesData);
            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult PreferenceSelections(PreferencesModel model)
        {
            var data = _tenantRepository.RetrievePreferences();
            
            data.ShopifyDataPullStart = model.ShopifyDataPullStart;
            data.AcumaticaTimeZone = model.AcumaticaTimeZone;
            data.AcumaticaDefaultItemClass = model.AcumaticaDefaultItemClass;
            data.AcumaticaDefaultPostingClass = model.AcumaticaDefaultPostingClass;
            data.AcumaticaPaymentMethod = model.AcumaticaPaymentMethod;
            data.AcumaticaPaymentCashAccount = model.AcumaticaPaymentCashAccount;
            data.AcumaticaTaxCategory = model.AcumaticaTaxCategory;
            data.AcumaticaTaxId = model.AcumaticaTaxId;
            data.AcumaticaTaxZone = model.AcumaticaTaxZone;

            _tenantRepository.SaveChanges();

            return JsonNetResult.Success();
        }



        // Warehouse synchronization
        //
        [HttpGet]
        public ActionResult Warehouses()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SyncWarehouses()
        {
            _hangfireService.SyncWarehouseAndLocation();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult WarehouseSyncStatus()
        {
            var isBackgroundJobRunning
                = _hangfireService.IsJobRunning(
                    BackgroundJobType.SyncWarehouseAndLocation);

            var state = _stateRepository.RetrieveSystemState();
            var warehouseSyncStatus
                = _statusService.WarehouseSyncStatus();

            var output = new WarehouseSyncStatusModel()
            {
                IsBackgroundJobRunning = isBackgroundJobRunning,
                WarehouseSyncState = state.WarehouseSync,
                Details = warehouseSyncStatus,
                IsRandomAccessMode = state.IsRandomAccessMode,
            };

            return new JsonNetResult(output);
        }



        // Inventory - to Acumatica
        //
        [HttpGet]
        public ActionResult InventoryToAcumatica()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult PushInventoryToAcumatica()
        {
            _hangfireService.PushInventoryToAcumatica();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult AcumaticaInventoryPushStatus()
        {
            var isRunning = 
                _hangfireService.IsJobRunning(
                    BackgroundJobType.PushInventoryToAcumatica);

            var state = _stateRepository.RetrieveSystemState();

            var output = new
            {
                IsBackgroundJobRunning = isRunning,
                SystemState = state.AcumaticaInventoryPush,
                IsRandomAccessMode = state.IsRandomAccessMode
            };

            return new JsonNetResult(output);
        }


        // Inventory - to Shopify
        //
        [HttpGet]
        public ActionResult InventoryToShopify()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PushInventoryToShopify()
        {
            _hangfireService.PushInventoryToShopify();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult ShopifyInventoryPushStatus()
        {
            var isRunning =
                _hangfireService
                    .IsJobRunning(BackgroundJobType.PushInventoryToShopify);

            var state = _stateRepository.RetrieveSystemState();

            var output = new
            {
                IsBackgroundJobRunning = isRunning,
                SystemState = state.ShopifyInventoryPush,
                IsRandomAccessMode = state.IsRandomAccessMode
            };

            return new JsonNetResult(output);
        }




        // Status inquiries
        // 
        [HttpGet]
        public ActionResult RealTime()
        {
            return View();
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

