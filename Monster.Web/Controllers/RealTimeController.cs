using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Monster.Acumatica.Api;
using Monster.Middle.Attributes;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Processes.Sync.Orders;
using Monster.Web.Models;
using Monster.Web.Models.Config;
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
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly OrderApi _orderApi;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly ShipmentClient _shipmentClient;
        private readonly IPushLogger _logger;

        public RealTimeController(
                StateRepository stateRepository,
                HangfireService hangfireService,
                ExecutionLogRepository logRepository, 
                SyncOrderRepository syncOrderRepository, 
                OrderApi orderApi, 
                SalesOrderClient salesOrderClient,
                ShipmentClient shipmentClient,
                IPushLogger logger)
        {
            _stateRepository = stateRepository;
            _hangfireService = hangfireService;            
            _logRepository = logRepository;
            _syncOrderRepository = syncOrderRepository;
            _orderApi = orderApi;
            _salesOrderClient = salesOrderClient;
            _shipmentClient = shipmentClient;
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


        [HttpPost]
        public ActionResult TriggerConfigDiagnosis()
        {
            _hangfireService.LaunchJob(JobType.Diagnostics);
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

