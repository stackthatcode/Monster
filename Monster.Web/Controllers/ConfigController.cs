﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Monster.Middle.Attributes;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Tenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Services;
using Monster.Middle.Processes.Sync.Model.Config;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Monster.Web.Models;
using Monster.Web.Models.Config;
using Monster.Web.Plumbing;
using Push.Foundation.Web.Json;


namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class ConfigController : Controller
    {
        private readonly ConnectionRepository _tenantRepository;
        private readonly SystemStateRepository _stateRepository;

        private readonly ExecutionLogService _logRepository;
        private readonly OneTimeJobService _oneTimeJobService;
        private readonly JobStatusService _jobStatusService;

        private readonly ConfigStatusService _statusService;
        private readonly ReferenceDataService _referenceDataService;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly AcumaticaBatchRepository _acumaticaBatchRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;

        public ConfigController(
                ConnectionRepository tenantRepository,
                SystemStateRepository stateRepository,

                ExecutionLogService logRepository,
                OneTimeJobService oneTimeJobService,
                JobStatusService jobStatusService,

                ConfigStatusService statusService, 
                ReferenceDataService referenceDataService, 

                PreferencesRepository preferencesRepository, 
                AcumaticaBatchRepository acumaticaBatchRepository, 
                SyncInventoryRepository syncInventoryRepository)
        {

            _tenantRepository = tenantRepository;
            _stateRepository = stateRepository;
            _oneTimeJobService = oneTimeJobService;

            _statusService = statusService;
            _referenceDataService = referenceDataService;
            _preferencesRepository = preferencesRepository;
            _logRepository = logRepository;
            _acumaticaBatchRepository = acumaticaBatchRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _jobStatusService = jobStatusService;
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
            var status = _statusService.AcumaticaConnectionStatus();
            var logs = _logRepository.RetrieveExecutionLogs().ToModel();
            var areAnyJobsRunning = _jobStatusService.AreAnyBackgroundJobsRunning();

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
            _oneTimeJobService.ConnectToAcumatica();
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
            _oneTimeJobService.PullAcumaticaRefData();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult AcumaticaRefDataStatus()
        {
            var status = _statusService.AcumaticaReferenceDataStatus();

            var logs = _logRepository.RetrieveExecutionLogs().ToModel();
            var areAnyJobsRunning = _jobStatusService.AreAnyBackgroundJobsRunning();

            var model = new
            {
                Status = status,
                IsJobRunning = areAnyJobsRunning,
                ExecutionLogs = logs,
            };
            
            return new JsonNetResult(model);
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
            
            // Automapping, anyone???
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
            state.PreferenceState = data.AreValid() ? SystemState.Ok : SystemState.Invalid;
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
            _oneTimeJobService.SyncWarehouseAndLocation();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult WarehouseSyncStatus()
        {
            var logs = _logRepository.RetrieveExecutionLogs().ToModel();
            var areAnyJobsRunning = _jobStatusService.AreAnyBackgroundJobsRunning();
            
            var state = _stateRepository.RetrieveSystemState();
            var details = _statusService.WarehouseSyncStatus();

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

                _statusService.UpdateWarehouseSyncStatus();

                transaction.Commit();
            }

            return JsonNetResult.Success();
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
            _oneTimeJobService.RunDiagnostics();
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult ConfigDiagnosisRunStatus()
        {
            var logs = _logRepository.RetrieveExecutionLogs().ToModel();
            var areAnyJobsRunning = _jobStatusService.AreAnyBackgroundJobsRunning();

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
            var output = _statusService.ConfigSummary();
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

