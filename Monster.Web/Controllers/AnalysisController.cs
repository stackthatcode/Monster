using System.Web.Mvc;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Analysis;
using Monster.Middle.Processes.Sync.Model.TaxTransfer;
using Monster.Middle.Processes.Sync.Services;
using Monster.Web.Attributes;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    [Authorize(Roles = "ADMIN, USER")]
    public class AnalysisController : Controller
    {
        private readonly ExecutionLogService _logRepository;
        private readonly AnalysisDataService _analysisDataService;
        private readonly PendingActionService _pendingActionService;
        private readonly InstanceContext _instanceContext;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        private readonly ShopifyUrlService _shopifyUrlService;

        public AnalysisController(
                ExecutionLogService logRepository,
                AnalysisDataService analysisDataService, 
                PendingActionService pendingActionService,
                InstanceContext instanceContext, 
                ShopifyOrderRepository shopifyOrderRepository, 
                ShopifyUrlService shopifyUrlService)
        {
            _logRepository = logRepository;
            _analysisDataService = analysisDataService;
            _pendingActionService = pendingActionService;
            _instanceContext = instanceContext;
            _shopifyOrderRepository = shopifyOrderRepository;
            _shopifyUrlService = shopifyUrlService;
        }


        [HttpGet]
        public ActionResult ExecutionLogs()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ExecutionLogData()
        {
            var logs = _logRepository.RetrieveExecutionLogs(1000);
            return new JsonNetResult(logs);
        }


        [HttpGet]
        public ActionResult OrderSync()
        {
            return View();
        }

        [HttpPost]
        public ActionResult OrderSyncResults(AnalyzerRequest request)
        {
            var grid = _analysisDataService.GetOrderAnalysisResults(request);
            var count = _analysisDataService.GetOrderAnalysisRecordCount(request);

            return new JsonNetResult(new {Grid = grid, Count = count});
        }

        [HttpGet]
        public ActionResult OrderAnalysis(long shopifyOrderId)
        {
            _instanceContext.InitializeAcumatica(HttpContext.GetIdentity().InstanceId);
            var financialSummary = _analysisDataService.GetOrderFinancialSummary(shopifyOrderId, true);
            var rootAction = _pendingActionService.Create(shopifyOrderId);

            var order = _shopifyOrderRepository.RetrieveOrder(shopifyOrderId);
            var taxTransfer = order.ToTaxTransfer();

            var shopifyDetail = new
            {
                ShopifyOrderId = shopifyOrderId,
                ShopifyOrderNbr = order.ShopifyOrderNumber,
                ShopifyOrderHref = _shopifyUrlService.ShopifyOrderUrl(shopifyOrderId),
                Transfer = taxTransfer,
            };

            return new JsonNetResult(new
            {
                FinancialSummary = financialSummary,
                ShopifyDetail = shopifyDetail,
                RootAction = rootAction,
            });
        }


        [HttpGet]
        public ActionResult ProductStockItem()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ProductStockItemResults(AnalyzerRequest request)
        {
            var stockItemResults = _analysisDataService.GetProductStockItemResults(request);
            var count = _analysisDataService.GetProductStockItemCount(request);

            return new JsonNetResult(new { Grid = stockItemResults, Count = count });
        }
    }
}

