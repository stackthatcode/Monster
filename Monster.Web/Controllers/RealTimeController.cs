using System.Linq;
using System.Web.Mvc;
using Monster.Middle.Attributes;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Orders;
using Monster.Middle.Processes.Sync.Status;
using Monster.Web.Models;
using Monster.Web.Models.RealTime;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Json;
using Push.Shopify.Api;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class RealTimeController : Controller
    {
        private readonly StateRepository _stateRepository;
        private readonly ExecutionLogRepository _logRepository;
        private readonly HangfireService _hangfireService;
        private readonly ShopifyInventoryRepository _shopifyInventoryRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly UrlService _urlService;
        private readonly IPushLogger _logger;

        public RealTimeController(
                StateRepository stateRepository,
                HangfireService hangfireService,
                ExecutionLogRepository logRepository, 
                SyncOrderRepository syncOrderRepository, 
                SyncInventoryRepository syncInventoryRepository,
                ShopifyInventoryRepository shopifyInventoryRepository,
                UrlService urlService,
                IPushLogger logger)
        {
            _stateRepository = stateRepository;
            _hangfireService = hangfireService;            
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
            var logDtos = logs.Select(x => new ExecutionLog(x)).ToList();

            var isConfigDiagnosisRunning 
                = _hangfireService.IsJobRunning(JobType.Diagnostics);

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
                IsRealTimeSyncRunning = _hangfireService.IsRealTimeSyncRunning(),
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
        



        // Inventory Sync Control
        //
        [HttpGet]
        public ActionResult InventorySyncControl()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RunInventoryPull ()
        {
            _hangfireService.LaunchJob(JobType.PullInventory);
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult InventoryPullStatus()
        {
            var isRunning =
                _hangfireService.IsJobRunning(JobType.PullInventory);

            var matchCount =
                _syncInventoryRepository.RetrieveVariantAndStockItemMatchCount();

            var state = _stateRepository.RetrieveSystemState();
            var logs = _logRepository.RetrieveExecutionLogs();
            var executionLogs = logs.Select(x => new ExecutionLog(x)).ToList();

            var output = new
            {
                IsBackgroundJobRunning = isRunning,
                Logs = executionLogs,
                SystemState = state.InventoryPull,
                HasMatchedInventory = matchCount > 0
            };

            return new JsonNetResult(output);
        }

        [HttpGet]
        public ActionResult VariantAndStockItemMatches()
        {
            var output = _syncInventoryRepository.RetrieveVariantAndStockItemMatches();
            return new JsonNetResult(output);
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
                    .Select(x => FilterInventoryModel
                        .Make(x, _urlService.ShopifyProductUrl)).ToList();
            
            return new JsonNetResult(output);
        }


        [HttpGet]
        public ActionResult ImportIntoShopify()
        {
            return View();
        }

    }
}

