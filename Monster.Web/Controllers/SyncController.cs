using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.State;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Status;
using Monster.Middle.Utility;
using Monster.Web.Attributes;
using Monster.Web.Models;
using Monster.Web.Models.Sync;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class SyncController : Controller
    {
        private readonly StateRepository _stateRepository;
        private readonly ExecutionLogService _logRepository;
        private readonly OneTimeJobScheduler _oneTimeJobService;
        private readonly RecurringJobScheduler _recurringJobService;
        private readonly JobMonitoringService _jobStatusService;
        private readonly ConfigStatusService _statusService;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly AcumaticaBatchRepository _acumaticaBatchRepository;
        private readonly EndToEndStatusService _endStatusService;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly UrlService _urlService;


        public SyncController(
                StateRepository stateRepository,
                OneTimeJobScheduler oneTimeJobService,
                RecurringJobScheduler recurringJobService,
                JobMonitoringService jobStatusService,
                ConfigStatusService statusService,
                ExecutionLogService logRepository,
                SyncInventoryRepository syncInventoryRepository,
                PreferencesRepository preferencesRepository,
                AcumaticaBatchRepository acumaticaBatchRepository,
                EndToEndStatusService endStatusService,
                UrlService urlService)
        {
            _stateRepository = stateRepository;
            _oneTimeJobService = oneTimeJobService;
            _recurringJobService = recurringJobService;
            _jobStatusService = jobStatusService;
            _statusService = statusService;
            _logRepository = logRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _urlService = urlService;
            _acumaticaBatchRepository = acumaticaBatchRepository;
            _endStatusService = endStatusService;
            _preferencesRepository = preferencesRepository;
        }

        
        // Status inquiries
        // 
        [HttpGet]
        public ActionResult EndToEnd()
        {
            return View();
        }

        [HttpPost]
        public ActionResult StartEndToEnd()
        {
            _recurringJobService.StartEndToEndSync();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult PauseEndToEnd()
        {
            _recurringJobService.KillEndToEndSync();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult EndToEndStatus()
        {
            var areAnyJobsRunning = _jobStatusService.AreAnyJobsRunning();
            var output = new EndToEndStatusModel();
            output.AreAnyJobsRunning = areAnyJobsRunning;

            if (areAnyJobsRunning)
            {
                var isEndToEndSyncRunning 
                        = _jobStatusService.IsJobRunning(BackgroundJobType.EndToEndSync);

                var logs = _logRepository.RetrieveExecutionLogs().ToModel();

                output.RunningStateModel = new RunningStateModel
                {
                    IsEndToEndSyncRunning = isEndToEndSyncRunning,
                    Logs = logs,
                };

                return new JsonNetResult(output);
            }
            else
            {
                var status = _endStatusService.GetEndToEndSyncStatus();

                output.NonRunningStateModel = new NonRunningStateModel
                {
                    IsStartingOrderReadyForEndToEnd = status.IsStartingOrderReadyForEndToEnd,
                    IsConfigReadyForEndToEnd = status.ConfigStateSummaryModel.IsConfigReadyForEndToEnd,
                    CanEndToEndSyncBeStarted = status.CanEndToEndSyncBeStarted,

                };

                return new JsonNetResult(output);
            }
        }



        // End-to-End Sync Settings
        //
        [HttpGet]
        public ActionResult SyncSettingsAndEnables()
        {
            var preferences = _preferencesRepository.RetrievePreferences();

            var output = new SyncSettingsModel();

            output.SyncOrdersEnabled = preferences.SyncOrdersEnabled;
            output.SyncInventoryEnabled = preferences.SyncInventoryEnabled;
            output.SyncRefundsEnabled = preferences.SyncRefundsEnabled;
            output.SyncShipmentsEnabled = preferences.SyncFulfillmentsEnabled;

            output.StartingOrderId = preferences.StartingShopifyOrderId;
            output.StartingOrderName 
                    = preferences.StartingShopifyOrderName.IsNullOrEmptyAlt("(not set)");
            output.StartOrderCreatedAtUtc
                = preferences.StartingShopifyOrderCreatedAtUtc?.ToString() ?? "(not set)";

            if (preferences.StartingShopifyOrderId.HasValue)
            {
                output.StartingOrderHref = _urlService.ShopifyOrderUrl(preferences.StartingShopifyOrderId.Value);
            }

            output.MaxParallelAcumaticaSyncs = preferences.MaxParallelAcumaticaSyncs;

            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult UpdateSyncEnables(SyncEnablesUpdateModel input)
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
            throw new NotImplementedException();

            var data = _preferencesRepository.RetrievePreferences();

            if (data.StartingShopifyOrderId.HasValue
                && data.StartingShopifyOrderId != model.StartingShopifyOrderId)
            {
                // TODO - are we sure about this...?
                _acumaticaBatchRepository.Reset();
            }

            //data.ShopifyOrderDateStart = model.ShopifyOrderDateStart;
            //data.ShopifyOrderNumberStart = model.ShopifyOrderNumberStart;
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
            var areAnyJobsRunning = _jobStatusService.AreAnyJobsRunning();

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
            _oneTimeJobService.RefreshInventory();
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

