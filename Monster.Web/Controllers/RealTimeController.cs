using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Monster.Middle.Attributes;
using Monster.Middle.Hangfire;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Monster.Middle.Processes.Sync.Status;
using Monster.Web.Models;
using Monster.Web.Models.RealTime;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class RealTimeController : Controller
    {
        private readonly SystemStateRepository _stateRepository;
        private readonly ExecutionLogService _logRepository;
        private readonly OneTimeJobService _oneTimeJobService;
        private readonly RecurringJobService _recurringJobService;
        private readonly JobStatusService _jobStatusService;
        private readonly ConfigStatusService _statusService;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly UrlService _urlService;
        private readonly IPushLogger _logger;

        public RealTimeController(
                SystemStateRepository stateRepository,
                OneTimeJobService oneTimeJobService,
                RecurringJobService recurringJobService,
                JobStatusService jobStatusService,
                ConfigStatusService statusService,
                ExecutionLogService logRepository,
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
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
            _recurringJobService.StartRoutineSync();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult PauseRealTime()
        {
            _recurringJobService.PauseRoutineSync();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult RealTimeStatus()
        {
            var areAnyJobsRunning = _jobStatusService.AreAnyBackgroundJobsRunning();
            var isRealTimeSyncRunning = _jobStatusService.IsRealTimeSyncRunning();

            var isConfigReadyForRealTime = 
                    _statusService.ConfigSummary().IsReadyForRealTimeSync;

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



        // Inventory Sync Control
        //
        [HttpGet]
        public ActionResult InventoryPullStatus()
        {
            var state = _stateRepository.RetrieveSystemStateNoTracking();
            var logs = _logRepository.RetrieveExecutionLogs().ToModel();
            var areAnyJobsRunning = _jobStatusService.AreAnyBackgroundJobsRunning();

            var output = new
            {
                AreAnyJobsRunning = areAnyJobsRunning,
                Logs = logs,
                SystemState = state.InventoryPullState,
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
                bool createInventoryReceipt, 
                bool enableInventorySync, 
                List<long> selectedSPIds)
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


        
        // TODO - move this to Status Service
        private OrderSyncSummary BuildOrderSummary()
        {
            var output = new OrderSyncSummary();
            output.TotalOrders = _syncOrderRepository.RetrieveTotalOrders();
            output.TotalOrdersWithSalesOrders = _syncOrderRepository.RetrieveTotalOrdersSynced();
            output.TotalOrdersWithShipments = _syncOrderRepository.RetrieveTotalOrdersOnShipments();
            output.TotalOrdersWithInvoices = _syncOrderRepository.RetrieveTotalOrdersInvoiced();
            return output;
        }
    }
}

