using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Monster.Middle.Attributes;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Middle.Processes.Sync.Status;
using Monster.Web.Models;
using Monster.Web.Models.Config;
using Push.Foundation.Web.Json;

namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class ConfigController : Controller
    {
        private readonly ConnectionRepository _tenantRepository;
        private readonly StateRepository _stateRepository;
        private readonly ExecutionLogRepository _logRepository;
        private readonly HangfireService _hangfireService;

        private readonly StatusService _statusService;
        private readonly ReferenceDataService _referenceDataService;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly AcumaticaBatchRepository _acumaticaBatchRepository;


        public ConfigController(
                ConnectionRepository tenantRepository,
                StateRepository stateRepository,
                HangfireService hangfireService,
                StatusService statusService, 
                ReferenceDataService referenceDataService, 
                PreferencesRepository preferencesRepository, 
                ExecutionLogRepository logRepository, 
                AcumaticaBatchRepository acumaticaBatchRepository)
        {

            _tenantRepository = tenantRepository;
            _stateRepository = stateRepository;
            _hangfireService = hangfireService;

            _statusService = statusService;
            _referenceDataService = referenceDataService;
            _preferencesRepository = preferencesRepository;
            _logRepository = logRepository;
            _acumaticaBatchRepository = acumaticaBatchRepository;
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
            _hangfireService.LaunchJob(JobType.ConnectToAcumatica);
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

            _hangfireService.LaunchJob(JobType.ConnectToAcumatica);
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
            _hangfireService.LaunchJob(JobType.PullAcumaticaRefData);
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
            var preferencesData = _preferencesRepository.RetrievePreferences();
            var output = Mapper.Map<PreferencesModel>(preferencesData);
            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult PreferenceSelections(PreferencesModel model)
        {
            var data = _preferencesRepository.RetrievePreferences();

            if (data.ShopifyOrderDateStart.HasValue &&
                data.ShopifyOrderDateStart.Value.Date !=
                model.ShopifyOrderDateStart.Value.Date)
            {
                _acumaticaBatchRepository.Reset();
            }
            
            data.ShopifyOrderDateStart = model.ShopifyOrderDateStart;
            data.ShopifyOrderNumberStart = model.ShopifyOrderNumberStart;
            data.AcumaticaTimeZone = model.AcumaticaTimeZone;
            data.AcumaticaDefaultItemClass = model.AcumaticaDefaultItemClass;
            data.AcumaticaDefaultPostingClass = model.AcumaticaDefaultPostingClass;
            data.AcumaticaPaymentMethod = model.AcumaticaPaymentMethod;
            data.AcumaticaPaymentCashAccount = model.AcumaticaPaymentCashAccount;
            data.AcumaticaTaxCategory = model.AcumaticaTaxCategory;
            data.AcumaticaTaxId = model.AcumaticaTaxId;
            data.AcumaticaTaxZone = model.AcumaticaTaxZone;

            _tenantRepository.SaveChanges();

            var state = _stateRepository.RetrieveSystemState();
            state.PreferenceSelections
                = data.AreValid() ? SystemState.Ok : SystemState.Invalid;
            _stateRepository.SaveChanges();

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
            _hangfireService.LaunchJob(JobType.SyncWarehouseAndLocation);
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult WarehouseSyncStatus()
        {
            var isBackgroundJobRunning
                = _hangfireService
                    .IsJobRunning(JobType.SyncWarehouseAndLocation);

            var state = _stateRepository.RetrieveSystemState();
            var warehouseSyncStatus = _statusService.WarehouseSyncStatus();

            var output = new WarehouseSyncStatusModel()
            {
                IsJobRunning = isBackgroundJobRunning,
                WarehouseSyncState = state.WarehouseSync,
                Details = warehouseSyncStatus,
                IsRandomAccessMode = state.IsRandomAccessMode,
            };

            return new JsonNetResult(output);
        }

        

        // Config Diagnostics
        //
        [HttpGet]
        public ActionResult Diagnostics()
        {
            var state = _stateRepository.RetrieveSystemState();
            state.IsRandomAccessMode = true;
            _stateRepository.SaveChanges();
            return View();
        }

        [HttpPost]
        public ActionResult TriggerConfigDiagnosis()
        {
            _hangfireService.LaunchJob(JobType.Diagnostics);
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult ConfigDiagnosisRunStatus()
        {
            var IsConfigDiagnosisRunning = _hangfireService.IsJobRunning(JobType.Diagnostics);
            var output = new { IsConfigDiagnosisRunning };
            return new JsonNetResult(output);
        }
        
        [HttpGet]
        public ActionResult ConfigDiagnosis()
        {
            var state = _stateRepository.RetrieveSystemState();
            var output = Mapper.Map<SystemStateSummaryModel>(state);
            output.IsReadyForRealTimeSync = state.IsReadyForRealTimeSync();

            return new JsonNetResult(output);
        }



        // *** Move to the Analyzers ***
        //
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

    }
}

