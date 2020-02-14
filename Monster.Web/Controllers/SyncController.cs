using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Misc.State;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Monster.Web.Attributes;
using Monster.Web.Models;
using Monster.Web.Models.Sync;
using Monster.Web.Plumbing;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Web.Json;
using Push.Shopify.Api;
using Push.Shopify.Api.Order;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    [Authorize(Roles = "ADMIN, USER")]
    public class SyncController : Controller
    {
        private readonly StateRepository _stateRepository;
        private readonly ExecutionLogService _logRepository;
        private readonly OneTimeJobScheduler _oneTimeJobService;
        private readonly RecurringJobScheduler _recurringJobService;
        private readonly JobMonitoringService _jobStatusService;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly SettingsRepository _settingsRepository;
        private readonly ConfigStatusService _configStatusService;
        private readonly InstanceContext _instanceContext;
        private readonly OrderApi _shopifyOrderApi;
        private readonly ShopifyUrlService _shopifyUrlService;
        private readonly AcumaticaUrlService _acumaticaUrlService;


        public SyncController(
                StateRepository stateRepository,
                OneTimeJobScheduler oneTimeJobService,
                RecurringJobScheduler recurringJobService,
                JobMonitoringService jobStatusService,
                ExecutionLogService logRepository,
                SyncInventoryRepository syncInventoryRepository,
                SettingsRepository settingsRepository,
                ShopifyUrlService shopifyUrlService,
                ConfigStatusService configStatusService,
                InstanceContext instanceContext, 
                AcumaticaUrlService acumaticaUrlService,
                OrderApi shopifyOrderApi)
        {
            _stateRepository = stateRepository;
            _oneTimeJobService = oneTimeJobService;
            _recurringJobService = recurringJobService;
            _jobStatusService = jobStatusService;
            _logRepository = logRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _shopifyUrlService = shopifyUrlService;
            _shopifyOrderApi = shopifyOrderApi;
            _instanceContext = instanceContext;
            _acumaticaUrlService = acumaticaUrlService;
            _configStatusService = configStatusService;
            _settingsRepository = settingsRepository;
        }

        
        // End-to-End Sync Control + Status
        // 
        [HttpGet]
        public ActionResult EndToEnd()
        {
            return View();
        }

        [HttpPost]
        public ActionResult StartEndToEndRecurring(int scheduleId)
        {
            var option = RecurringSchedule.Options.First(x => x.Id == scheduleId);
            _logRepository.Log($"Scheduling Recurring End-to-End Sync - run {option.Desc}");
            _recurringJobService.StartEndToEndSync(option.Cron);

            var settings = _settingsRepository.RetrieveSettings();
            settings.LastRecurringSchedule = scheduleId;
            _settingsRepository.SaveChanges();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult PauseEndToEndRecurring()
        {
            _recurringJobService.KillEndToEndSync();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult RunEndToEndImmediately()
        {
            _oneTimeJobService.EndToEndSync();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult RunEndToEndImmediatelySingleOrder(long shopifyOrderId)
        {
            _oneTimeJobService.EndToEndSyncSingleShopifyOrder(shopifyOrderId);
            return JsonNetResult.Success();
        }


        [HttpGet]
        public ActionResult EndToEndStatus()
        {
            var status = _configStatusService.GetConfigStatusSummary();
            var isRecurringEndToEndActive = _recurringJobService.IsEndToEndSyncActive();
            var settings = _settingsRepository.RetrieveSettings();
            var recurringSchedule =
                RecurringSchedule.Options.First(x => x.Id == settings.LastRecurringSchedule).Desc;

            var output = new 
            {
                status.IsStartingOrderReady,
                status.IsConfigReady,
                status.CanStartEndToEnd,
                IsRecurringEndToEndActive = isRecurringEndToEndActive,
                RecurringSchedule = recurringSchedule,
            };

            return new JsonNetResult(output);
        }



        // End-to-End Sync Settings
        //
        [HttpGet]
        public ActionResult SyncSettingsAndEnables()
        {
            var settings = _settingsRepository.RetrieveSettings();

            var output = new SyncSettingsModel();
            output.PullFromAcumaticaEnabled = settings.PullFromAcumaticaEnabled;
            output.PullFromShopifyEnabled = settings.PullFromShopifyEnabled;

            output.SyncOrdersEnabled = settings.SyncOrdersEnabled;
            output.SyncInventoryEnabled = settings.SyncInventoryEnabled;
            output.SyncRefundsEnabled = settings.SyncRefundsEnabled;
            output.SyncShipmentsEnabled = settings.SyncFulfillmentsEnabled;

            output.StartingOrderId = settings.ShopifyOrderId;
            output.StartingOrderName = settings.ShopifyOrderName.IsNullOrEmptyAlt("(not set)");
            output.StartOrderCreatedAtUtc = settings.ShopifyOrderCreatedAtUtc?.ToString() ?? "(not set)";

            if (settings.ShopifyOrderId.HasValue)
            {
                output.StartingOrderHref = _shopifyUrlService.ShopifyOrderUrl(settings.ShopifyOrderId.Value);
            }

            output.ReleasePaymentsOnSync = settings.ReleasePaymentsOnSync;
            output.MaxParallelAcumaticaSyncs = settings.MaxParallelAcumaticaSyncs;
            output.MaxNumberOfOrders = settings.MaxNumberOfOrders;
            output.ShopifyDelayMs = settings.ShopifyDelayMs;

            output.InventorySyncAvailableQty = settings.InventorySyncAvailableQty;
            output.InventorySyncPrice = settings.InventorySyncPrice;
            output.InventorySyncWeight = settings.InventorySyncWeight;

            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult UpdateSyncEnables(SyncEnablesUpdateModel input)
        {
            var data = _settingsRepository.RetrieveSettings();
            data.PullFromShopifyEnabled = input.PullFromShopifyEnabled;
            data.PullFromAcumaticaEnabled = input.PullFromAcumaticaEnabled;
            data.SyncOrdersEnabled = input.SyncOrdersEnabled;
            data.SyncInventoryEnabled = input.SyncInventoryEnabled;
            data.SyncRefundsEnabled = input.SyncRefundsEnabled;
            data.SyncFulfillmentsEnabled = input.SyncShipmentsEnabled;
            _settingsRepository.SaveChanges();

            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult VerifyShopifyOrder(string orderNameOrId)
        {
            var identity = HttpContext.GetIdentity();
            _instanceContext.InitializeShopify();

            var jsonByName = _shopifyOrderApi.RetrieveByName(orderNameOrId);
            var ordersByName = jsonByName.DeserializeToOrderList().orders;
            if (ordersByName.Count > 0)
            {
                return new JsonNetResult(MakeOrderVerification(ordersByName.First()));
            }

            if (orderNameOrId.IsLong())
            {
                var jsonById = _shopifyOrderApi.Retrieve(orderNameOrId.ToLong());
                var orderById = jsonById.DeserializeToOrderParent().order;
                if (orderById != null)
                {
                    return new JsonNetResult(orderById);
                }
            }

            return new JsonNetResult(OrderVerification.Empty());
        }

        private OrderVerification MakeOrderVerification(Order order)
        {
            var output = new OrderVerification();
            output.ShopifyOrderId = order.id;
            output.ShopifyOrderName = order.name;
            output.ShopifyOrderCreatedAtUtc = order.created_at.ToUniversalTime().DateTime.ToString();
            output.ShopifyOrderHref = _shopifyUrlService.ShopifyOrderUrl(order.id);

            return output;
        }

        [HttpPost]
        public ActionResult OrderSyncSettingsUpdate(OrderSyncSettingsModel model)
        {
            var data = _settingsRepository.RetrieveSettings();
            if (!data.ShopifyOrderId.HasValue && model.ShopifyOrderId.HasValue)
            {
                data.ShopifyOrderId = model.ShopifyOrderId;
                data.ShopifyOrderName = model.ShopifyOrderName;
                data.ShopifyOrderCreatedAtUtc = model.ShopifyOrderCreatedAtUtc;

                _stateRepository.UpdateSystemState(x => x.StartingShopifyOrderState, StateCode.Ok);
            }

            data.ReleasePaymentsOnSync = model.ReleasePaymentsOnSync;

            data.MaxParallelAcumaticaSyncs = model.MaxParallelAcumaticaSyncs;
            data.MaxNumberOfOrders
                = model.MaxNumberOfOrders == 0 
                    ? SettingsRepository.DefaultMaxNumberOfOrders : model.MaxNumberOfOrders;
            data.ShopifyDelayMs = model.ShopifyDelayMs;

            _settingsRepository.SaveChanges();

            //// TODO - are we sure about this...?
            //_acumaticaBatchRepository.Reset();

            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult InventorySettingsUpdate(InventorySyncSettingsUpdateModel model)
        {
            var data = _settingsRepository.RetrieveSettings();
            data.InventorySyncAvailableQty = model.InventorySyncAvailableQty;
            data.InventorySyncPrice = model.InventorySyncPrice;
            data.InventorySyncWeight = model.InventorySyncWeight;
            _settingsRepository.SaveChanges();
            return JsonNetResult.Success();
        }



        // Inventory Sync Control
        //
        [HttpGet]
        public ActionResult InventoryRefreshStatus()
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
        public ActionResult RunInventoryRefresh()
        {
            _oneTimeJobService.RefreshInventory();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult VariantAndStockItemMatches(
                string filterText, int syncEnabledFilter, int pageNumber, int pageSize)
        {
            var startingRecord = PagingHelper.StartingRecord(pageNumber, pageSize);

            var results = 
                _syncInventoryRepository
                    .SearchSyncedStockItemsResults(filterText, syncEnabledFilter, startingRecord, pageSize);

            var resultsDtos = 
                results.Select(item => 
                    VariantAndStockItemDto.Make(
                        item, 
                        _shopifyUrlService.ShopifyVariantUrl, 
                        _acumaticaUrlService.AcumaticaStockItemUrl))
                    .ToList();

            var count = _syncInventoryRepository.SearchSyncedStockItemsCount(filterText, syncEnabledFilter);

            var output = new
            {
                results = resultsDtos,
                count = count,
            };

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



        // Shopify Product Search method
        //

        [HttpGet]
        public ActionResult FilterShopifyProducts(
                string terms = "", bool onlyHavingUnsyncedVariants = false,  int maxRecords = 1000)
        {
            var searchRecords = 
                _syncInventoryRepository.ProductSearchRecords(terms, onlyHavingUnsyncedVariants, 0, maxRecords);

            var searchResult = searchRecords
                .Select(x => ShopifyProductModel.Make(x, _shopifyUrlService.ShopifyProductUrl)).ToList();

            var searchCount = _syncInventoryRepository.ProductSearchCount(terms, onlyHavingUnsyncedVariants);

            var output = new { searchResult, searchCount, };
            return new JsonNetResult(output);
        }



        // Import Into Acumatica
        //
        [HttpGet]
        public ActionResult ImportIntoAcumatica()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ProductDetail(long shopifyProductId)
        {
            var product = _syncInventoryRepository.RetrieveProduct(shopifyProductId);
            var output = ShopifyProductModel
                .Make(product, _shopifyUrlService.ShopifyProductUrl, includeVariantGraph: true);
            return new JsonNetResult(output);
        }

        [HttpGet]
        public ActionResult SyncedWarehouses()
        {
            var warehouses = 
                _syncInventoryRepository
                    .RetrieveMatchedWarehouses()
                    .Select(x => x.AcumaticaWarehouseId)
                    .ToList();
            return new JsonNetResult(warehouses);
        }

        [HttpPost]
        public ActionResult RunImportIntoAcumatica(
                bool createInventoryReceipt, string warehouseId, bool enableInventorySync, List<long> selectedSPIds)
        {
            _oneTimeJobService
                .ImportAcumaticaStockItems(
                    selectedSPIds, createInventoryReceipt, warehouseId, enableInventorySync);

            return JsonNetResult.Success();
        }


        [HttpPost]
        public ActionResult RunSyncWithAcumatica(bool enableInventorySync, List<long> selectedSPIds)
        {
            _oneTimeJobService.SyncAcumaticaStockItems(selectedSPIds, enableInventorySync);

            return JsonNetResult.Success();
        }




        // Import into Shopify methods
        //
        [HttpGet]
        public ActionResult ImportIntoShopify()
        {
            var settings = _settingsRepository.RetrieveSettings();

            if (settings.AcumaticaTaxableCategory.IsNullOrEmpty() ||
                settings.AcumaticaTaxExemptCategory.IsNullOrEmpty())
            {
                return Redirect(GlobalConfig.Url("/Config/SettingsTaxes"));
            }

            var model = new Monster.Web.Models.Sync.ImportIntoShopifyModel
            {
                TaxableCategory = settings.AcumaticaTaxableCategory,
                ExemptCategory = settings.AcumaticaTaxExemptCategory,
            };
            return View(model);
        }
        
        [HttpGet]
        public ActionResult FilterAcumaticaStockItems(string terms = "", int maxRecords = 1000)
        {
            var searchRecords = _syncInventoryRepository.StockItemSearchRecords(terms,0, maxRecords);
            var searchResult = searchRecords
                .Select(x => AcumaticaStockItemModel.Make(x, _acumaticaUrlService.AcumaticaStockItemUrl))
                .ToList();

            var searchCount = _syncInventoryRepository.StockItemSearchCount(terms);

            var output = new {searchResult, searchCount,};
            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult RunImportToCreateNewProduct(
                List<string> acumaticaItemIds, string title, string productType, string vendor)
        {
            var context = new ShopifyNewProductImportContext();
            context.ProductTitle = title;
            context.ProductVendor = vendor;
            context.ProductType = productType;
            context.AcumaticaItemIds = acumaticaItemIds;
            _oneTimeJobService.ImportNewShopifyProduct(context);

            return JsonNetResult.Success();
        }


        [HttpPost]
        public ActionResult RunImportIntoExistingProduct(long shopifyProductId, List<string> acumaticaItemIds)
        {
            var context = new ShopifyAddVariantImportContext();
            context.ShopifyProductId = shopifyProductId;
            context.AcumaticaItemIds = acumaticaItemIds;
            _oneTimeJobService.ImportAddShopifyVariantsToProduct(context);

            return JsonNetResult.Success();
        }
    }
}

