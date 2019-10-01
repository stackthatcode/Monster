using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Misc;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Monster.Middle.Processes.Sync.Status;
using Monster.Web.Attributes;
using Monster.Web.Models;
using Monster.Web.Models.Config;
using Monster.Web.Models.RealTime;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class RealTimeController : Controller
    {
        private readonly StateRepository _stateRepository;
        private readonly ExecutionLogService _logRepository;
        private readonly OneTimeJobService _oneTimeJobService;
        private readonly RecurringJobService _recurringJobService;
        private readonly JobMonitoringService _jobStatusService;
        private readonly ConfigStatusService _statusService;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly AcumaticaBatchRepository _acumaticaBatchRepository;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly UrlService _urlService;
        private readonly IPushLogger _logger;

        public RealTimeController(
                StateRepository stateRepository,
                OneTimeJobService oneTimeJobService,
                RecurringJobService recurringJobService,
                JobMonitoringService jobStatusService,
                ConfigStatusService statusService,
                ExecutionLogService logRepository,
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
                PreferencesRepository preferencesRepository,
                AcumaticaBatchRepository acumaticaBatchRepository,
                UrlService urlService,
                IPushLogger logger)
        {
            _stateRepository = stateRepository;
            _oneTimeJobService = oneTimeJobService;
            _recurringJobService = recurringJobService;
            _jobStatusService = jobStatusService;
            _statusService = statusService;
            _logRepository = logRepository;
            _syncOrderRepository = syncOrderRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _urlService = urlService;
            _logger = logger;
            _acumaticaBatchRepository = acumaticaBatchRepository;
            _preferencesRepository = preferencesRepository;
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
            _recurringJobService.StartEndToEndSync();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult PauseRealTime()
        {
            _recurringJobService.KillEndToEndSync();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult RealTimeStatus()
        {
            var monitoringDigest = _jobStatusService.GetMonitoringDigest();
            var isRealTimeSyncRunning = monitoringDigest.IsJobTypeActive(BackgroundJobType.EndToEndSync);
            var areAnyJobsRunning = monitoringDigest.AreAnyJobsActive;    

            var isConfigReadyForRealTime = _statusService.ConfigSummary().IsReadyForRealTimeSync;

            var logs = _logRepository.RetrieveExecutionLogs().ToModel();

            var output = new
            {
                AreAnyJobsRunning = areAnyJobsRunning,
                IsRealTimeSyncRunning = isRealTimeSyncRunning,
                IsConfigReadyForRealTime = isConfigReadyForRealTime,
                Logs = logs,
            };

            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult EndToEnd()
        {
            //_oneTimeJobService.();
            return JsonNetResult.Success();
        }


        // Real-Time Settings
        //
        [HttpGet]
        public ActionResult SyncSettingsAndEnables()
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var SyncEnablesModel = Mapper.Map<SyncEnablesModel>(preferences);
            var OrderSyncSettingsModel = Mapper.Map<OrderSyncSettingsModel>(preferences);

            var output = new
            {
                SyncEnablesModel,
                OrderSyncSettingsModel,
            };

            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult SyncEnablesUpdate(SyncEnablesModel input)
        {
            var data = _preferencesRepository.RetrievePreferences();
            data.SyncOrdersEnabled = input.SyncOrdersEnabled;
            data.SyncInventoryEnabled = input.SyncInventoryEnabled;
            data.SyncRefundsEnabled = input.SyncRefundsEnabled;
            data.SyncFulfillmentsEnabled = input.SyncShipmentsEnabled;
            _preferencesRepository.SaveChanges();

            return JsonNetResult.Success();
        }
        
        [HttpPost]
        public ActionResult OrderSyncSettingsUpdate(OrderSyncSettingsModel model)
        {
            var data = _preferencesRepository.RetrievePreferences();

            if (data.ShopifyOrderDateStart.HasValue 
                    && data.ShopifyOrderDateStart.Value.Date 
                    != model.ShopifyOrderDateStart.Value.Date)
            {
                _acumaticaBatchRepository.Reset();
            }

            data.ShopifyOrderDateStart = model.ShopifyOrderDateStart;
            data.ShopifyOrderNumberStart = model.ShopifyOrderNumberStart;
            data.MaxParallelAcumaticaSyncs = model.MaxParallelAcumaticaSyncs;
            _preferencesRepository.SaveChanges();

            return JsonNetResult.Success();
        }
        


        // Inventory Sync Control
        //
        [HttpGet]
        public ActionResult InventoryPullStatus()
        {
            var state = _stateRepository.RetrieveSystemStateNoTracking();
            var logs = _logRepository.RetrieveExecutionLogs().ToModel();
            var areAnyJobsRunning = _jobStatusService.GetMonitoringDigest();

            var output = new
            {
                AreAnyJobsRunning = areAnyJobsRunning,
                Logs = logs,
                SystemState = state.InventoryRefreshState,
            };

            return new JsonNetResult(output);
        }
        
        [HttpGet]
        public ActionResult InventorySyncControl()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RunInventoryPull()
        {
            _oneTimeJobService.PullInventory();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult VariantAndStockItemMatches(string filterText, int syncEnabledFilter)
        {
            var results = 
                _syncInventoryRepository
                    .SearchVariantAndStockItems(filterText, syncEnabledFilter);

            var output = 
                results.Select(item => 
                    VariantAndStockItemDto.Make(
                        item, _urlService.ShopifyVariantUrl, _urlService.AcumaticaStockItemUrl))
                    .ToList();

            return new JsonNetResult(output);
        }
        
        [HttpPost]
        public ActionResult SyncEnabled(long monsterVariantId, bool syncEnabled)
        {
            _syncInventoryRepository.UpdateVariantSync(monsterVariantId, syncEnabled);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult BulkSyncEnabled(List<long> monsterVariantIds, bool syncEnabled)
        {
            _syncInventoryRepository.UpdateVariantSync(monsterVariantIds, syncEnabled);
            return JsonNetResult.Success();

        }

        


        // Inventory loading tools
        //
        [HttpGet]
        public ActionResult ImportIntoAcumatica()
        {
            return View();
        }

        public ActionResult FilterInventory(string terms = "")
        {
            var searchResult = _syncInventoryRepository.ProductSearch(terms);
            var output
                = searchResult
                    .Select(x => ShopifyProductModel
                        .Make(x, _urlService.ShopifyProductUrl)).ToList();

            return new JsonNetResult(output);
        }

        public ActionResult SyncedWarehouses()
        {
            var locations = _syncInventoryRepository.RetrieveLocations();
            var output = LocationsModel.Make(locations);
            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult RunImportIntoAcumatica(
                bool createInventoryReceipt, bool enableInventorySync, List<long> selectedSPIds)
        {
            _oneTimeJobService
                .ImportIntoAcumatica(
                    selectedSPIds, createInventoryReceipt, enableInventorySync);

            return JsonNetResult.Success();
        }
        
        [HttpGet]
        public ActionResult ProductDetail(long shopifyProductId)
        {
            var product = _syncInventoryRepository.RetrieveProduct(shopifyProductId);
            var output = ShopifyProductModel.Make(
                product, _urlService.ShopifyProductUrl, includeVariantGraph: true);
            return new JsonNetResult(output);
        }

        

        // Import into Shopify functions...
        //
        [HttpGet]
        public ActionResult ImportIntoShopify()
        {
            return View();
        }


    }
}

