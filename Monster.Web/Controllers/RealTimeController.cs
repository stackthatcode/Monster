using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Monster.Acumatica.Http;
using Monster.Middle.Attributes;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Processes.Sync.Orders;
using Monster.Middle.Security;
using Monster.Web.Models;
using Monster.Web.Models.Config;
using Monster.Web.Models.RealTime;
using Push.Foundation.Web.Json;
using Push.Shopify.Http;

namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class RealTimeController : Controller
    {
        private readonly ConnectionRepository _tenantRepository;
        private readonly StateRepository _stateRepository;
        private readonly ExecutionLogRepository _logRepository;
        private readonly HangfireService _hangfireService;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly AcumaticaHttpContext _acumaticaHttpContext;
        private readonly ShopifyHttpContext _shopifyHttpContext;

        public RealTimeController(
                ConnectionRepository tenantRepository,
                StateRepository stateRepository,
                HangfireService hangfireService,
                ExecutionLogRepository logRepository, 
                SyncOrderRepository syncOrderRepository)
        {

            _tenantRepository = tenantRepository;
            _stateRepository = stateRepository;
            _hangfireService = hangfireService;            
            _logRepository = logRepository;
            _syncOrderRepository = syncOrderRepository;
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
                = _hangfireService.IsJobRunning(BackgroundJobType.Diagnostics);

            var orderSyncView = _syncOrderRepository.RetrieveOrderSyncView();
            
            //var orderSyncModel = 


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

