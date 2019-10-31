﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Monster.Middle.Misc.External;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.State;
using Monster.Middle.Processes.Acumatica.Services;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Monster.Web.Attributes;
using Monster.Web.Models;
using Monster.Web.Models.Config;
using Monster.Web.Plumbing;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class ConfigController : Controller
    {
        private readonly ExternalServiceRepository _connectionRepository;
        private readonly StateRepository _stateRepository;

        private readonly ExecutionLogService _logRepository;
        private readonly OneTimeJobScheduler _oneTimeJobService;
        private readonly JobMonitoringService _jobStatusService;

        private readonly ConfigStatusService _statusService;
        private readonly CombinedRefDataService _combinedRefDataService;
        private readonly SettingsRepository _settingsRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;

        public ConfigController(
                ExternalServiceRepository connectionRepository,
                StateRepository stateRepository,

                ExecutionLogService logRepository,
                OneTimeJobScheduler oneTimeJobService,
                JobMonitoringService jobStatusService,

                ConfigStatusService statusService, 
                CombinedRefDataService combinedRefDataService, 

                SettingsRepository settingsRepository, 
                SyncInventoryRepository syncInventoryRepository)
        {

            _connectionRepository = connectionRepository;
            _stateRepository = stateRepository;
            _oneTimeJobService = oneTimeJobService;

            _statusService = statusService;
            _combinedRefDataService = combinedRefDataService;
            _settingsRepository = settingsRepository;
            _logRepository = logRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _jobStatusService = jobStatusService;
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
            var status = _statusService.GetAcumaticaConnectionStatus();
            var logs = _logRepository.RetrieveExecutionLogs().ToModel();
            var areAnyJobsRunning = _jobStatusService.AreAnyJobsRunning();

            var model = new
            {
                Status = status,
                IsJobRunning = areAnyJobsRunning,
                ExecutionLogs = logs,
            };
            
            return new JsonNetResult(model);
        }

        [HttpGet]
        public ActionResult AcumaticaCredentials()
        {
            var tenant = _connectionRepository.Retrieve();

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
            _oneTimeJobService.ConnectToAcumatica();
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult AcumaticaCredentials(AcumaticaCredentialsModel model)
        {
            var state = _stateRepository.RetrieveSystemStateNoTracking();

            if (state.AcumaticaConnState != StateCode.None)
            {
                _connectionRepository
                    .UpdateAcumaticaCredentials(model.UserName, model.Password);
            }
            else
            {
                _connectionRepository
                    .UpdateAcumaticaCredentials(
                        model.InstanceUrl,
                        model.Company,
                        model.Branch,
                        model.UserName,
                        model.Password);
            }

            _oneTimeJobService.ConnectToAcumatica();
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
            _oneTimeJobService.RefreshAcumaticaReferenceData();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult AcumaticaRefDataStatus()
        {
            var status = _statusService.GetAcumaticaReferenceDataStatus();

            var logs = _logRepository.RetrieveExecutionLogs().ToModel();
            var areAnyJobsRunning = _jobStatusService.AreAnyJobsRunning();


            var model = new
            {
                Status = status,
                IsJobRunning = areAnyJobsRunning,
                ExecutionLogs = logs,
                IsRandomAccessMode = status.IsRandomAccessMode,
            };
            
            return new JsonNetResult(model);
        }
        

        // Preference-selection of Reference Data
        //
        [HttpGet]
        public ActionResult Settings()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AcumaticaReferenceData()
        {
            var data = _combinedRefDataService.Retrieve();
            return new JsonNetResult(data);
        }

        [HttpGet]
        public ActionResult PreferenceSelections()
        {
            var preferencesData = _settingsRepository.RetrieveSettings();
            var output = new SettingsModel();
            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult PreferenceSelections(SettingsModel model)
        {
            var data = _settingsRepository.RetrieveSettings();
            
            data.AcumaticaTimeZone = model.AcumaticaTimeZone;
            data.AcumaticaDefaultItemClass = model.AcumaticaDefaultItemClass;
            data.AcumaticaDefaultPostingClass = model.AcumaticaDefaultPostingClass;
            data.AcumaticaTaxCategory = model.AcumaticaTaxCategory;
            data.AcumaticaTaxId = model.AcumaticaTaxId;
            data.AcumaticaTaxZone = model.AcumaticaTaxZone;

            _connectionRepository.SaveChanges();

            _statusService.RefreshSettingsStatus();

            return JsonNetResult.Success();
        }


        // Payment Methods
        //
        [HttpGet]
        public ActionResult PaymentMethods()
        {
            return View();
        }

        [HttpGet]
        public ActionResult PaymentMethodData()
        {
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
            _oneTimeJobService.SyncWarehouseAndLocation();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult WarehouseSyncStatus()
        {
            var logs = _logRepository.RetrieveExecutionLogs().ToModel();
            var areAnyJobsRunning = _jobStatusService.AreAnyJobsRunning();

            var state = _stateRepository.RetrieveSystemStateNoTracking();
            var details = _statusService.GetWarehouseSyncStatus();

            var output = new WarehouseSyncStatusModel()
            {
                IsJobRunning = areAnyJobsRunning,
                ExecutionLogs = logs,

                WarehouseSyncState = state.WarehouseSyncState,
                IsRandomAccessMode = state.IsRandomAccessMode,
                Details = details,
            };

            return new JsonNetResult(output);
        }

        [HttpGet]
        public ActionResult WarehouseSyncData()
        {
            var locations = _syncInventoryRepository.RetrieveLocations();
            var warehouses = _syncInventoryRepository.RetrieveWarehouses();

            var output = new
            {
                ActivatedLocations = 
                    locations
                        .Where(x => x.ShopifyActive)
                        .Select(x => ShopifyLocationModel.Make(x))
                        .ToList(),

                DeactivatedLocations =
                    locations
                        .Where(x => !x.ShopifyActive)
                        .Select(x => ShopifyLocationModel.Make(x))
                        .ToList(),

                Warehouses = warehouses.Select(x => AcumaticaWarehouseModel.Make(x)),
            };

            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult WarehouseSyncDataUpdate(List<AcumaticaWarehouseModel> input)
        {
            using (var transaction = _syncInventoryRepository.BeginTransaction())
            {
                foreach (var item in input)
                {
                    _syncInventoryRepository.ImprintWarehouseSync(
                            item.AcumaticaWarehouseId, item.ShopifyLocationId);
                }

                _statusService.RefreshWarehouseSyncStatus();

                transaction.Commit();
            }

            return JsonNetResult.Success();
        }



        // Config Diagnostics
        //
        [HttpGet]
        public ActionResult Diagnostics()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TriggerConfigDiagnosis()
        {
            _oneTimeJobService.RunDiagnostics();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult ConfigDiagnosisRunStatus()
        {
            var logs = _logRepository.RetrieveExecutionLogs().ToModel();
            var areAnyJobsRunning = _jobStatusService.AreAnyJobsRunning();

            var model = new
            {
                IsJobRunning = areAnyJobsRunning,
                ExecutionLogs = logs,
            };
            
            return new JsonNetResult(model);
        }
        
        [HttpGet]
        public ActionResult ConfigDiagnosis()
        {
            var output = _statusService.GetConfigStatusSummary();
            return new JsonNetResult(output);
        }

        [HttpGet]
        public ActionResult Complete()
        {
            _stateRepository.UpdateIsRandomAccessMode(true);
            return Redirect(GlobalConfig.Url("Config/CompleteScreen"));
        }

        [HttpGet]
        public ActionResult CompleteScreen()
        {
            return View();
        }
    }
}

