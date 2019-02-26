using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Monster.Middle.Attributes;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Processes.Sync.Orders;
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
        private readonly StateRepository _stateRepository;
        private readonly ExecutionLogRepository _logRepository;
        private readonly OneTimeJobService _oneTimeJobService;
        private readonly ShopifyInventoryRepository _shopifyInventoryRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly UrlService _urlService;
        private readonly IPushLogger _logger;

        public RealTimeController(
            StateRepository stateRepository,
            OneTimeJobService oneTimeJobService,
            ExecutionLogRepository logRepository,
            SyncOrderRepository syncOrderRepository,
            SyncInventoryRepository syncInventoryRepository,
            ShopifyInventoryRepository shopifyInventoryRepository,
            UrlService urlService,
            IPushLogger logger)
        {
            _stateRepository = stateRepository;
            _oneTimeJobService = oneTimeJobService;
            _logRepository = logRepository;
            _syncOrderRepository = syncOrderRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _shopifyInventoryRepository = shopifyInventoryRepository;
            _urlService = urlService;
            _logger = logger;
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

        [HttpPost]
        public ActionResult StartRealTime()
        {
            _oneTimeJobService.StartRoutineSync();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult PauseRealTime()
        {
            _oneTimeJobService.PauseRoutineSync();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult RealTimeStatus()
        {
            var logs = _logRepository.RetrieveExecutionLogs();
            var logDtos = logs.Select(x => new ExecutionLog(x)).ToList();

            var isConfigDiagnosisRunning
                = _oneTimeJobService.IsJobRunning(BackgroundJobType.Diagnostics);

            var orderSyncView = _syncOrderRepository.RetrieveOrderSyncView();

            //foreach (var row in orderSyncView)
            //{
            //    //row.ShopifyOrderUrl = _orderApi.OrderInterfaceUrlById(row.ShopifyOrderId);

            //    //if (row.AcumaticaOrderNbr.HasValue())
            //    //{
            //    //    row.AcumaticaOrderUrl = _orderApi.OrderInterfaceUrlById(row.ShopifyOrderId);
            //    //}
            //    //if (row.AcumaticaShipmentNbr.HasValue())
            //    //{
            //    //    row.AcumaticaShipmentUrl = _shipmentClient.ShipmentUrl(row.AcumaticaShipmentNbr);
            //    //}

            //    _logger.Debug(_orderApi.OrderInterfaceUrlById(row.ShopifyOrderId));
            //    _logger.Debug(_salesOrderClient.OrderInterfaceUrlById(row.AcumaticaOrderNbr));
            //    _logger.Debug(_shipmentClient.ShipmentUrl(row.AcumaticaShipmentNbr));
            //    _logger.Debug(_salesOrderClient.OrderInterfaceUrlById(row.AcumaticaOrderNbr));
            //}


            var output = new
            {
                IsRealTimeSyncRunning = _oneTimeJobService.IsRealTimeSyncRunning(),
                IsConfigDiagnosisRunning = isConfigDiagnosisRunning,
                Logs = logDtos,
                OrderSummary = BuildOrderSummary(),
                OrderSyncView = orderSyncView,
            };

            return new JsonNetResult(output);
        }

        private OrderSyncSummary BuildOrderSummary()
        {
            var output = new OrderSyncSummary();
            output.TotalOrders = _syncOrderRepository.RetrieveTotalOrders();
            output.TotalOrdersWithSalesOrders = _syncOrderRepository.RetrieveTotalOrdersSynced();
            output.TotalOrdersWithShipments = _syncOrderRepository.RetrieveTotalOrdersOnShipments();
            output.TotalOrdersWithInvoices = _syncOrderRepository.RetrieveTotalOrdersInvoiced();
            return output;
        }



        // *** Monitors Status on all Inventory Jobs
        [HttpGet]
        public ActionResult StatusForAllInventoryJobs()
        {
            var areAnyJobsRunning
                = _oneTimeJobService.AreAnyJobsRunning(
                    new List<int>
                    {
                        BackgroundJobType.PullInventory,
                        BackgroundJobType.ImportIntoAcumatica,
                    });

            var state = _stateRepository.RetrieveSystemState();
            var logs = _logRepository.RetrieveExecutionLogs();
            var executionLogs = logs.Select(x => new ExecutionLog(x)).ToList();

            var output = new
            {
                SystemState = state.InventoryPull,
                AreAnyJobsRunning = areAnyJobsRunning,
                Logs = executionLogs,
            };

            return new JsonNetResult(output);
        }



        // Inventory Sync Control
        //
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
        [HttpGet]
        public ActionResult ImportIntoShopify()
        {
            return View();
        }

    }
}

