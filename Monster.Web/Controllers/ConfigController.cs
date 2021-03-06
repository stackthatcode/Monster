﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Monster.Middle.Misc.External;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Misc.State;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Services;
using Monster.Middle.Processes.Sync.Model.Settings;
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
    [Authorize(Roles = "ADMIN, USER")]
    public class ConfigController : Controller
    {
        private readonly CredentialsRepository _connectionRepository;
        private readonly StateRepository _stateRepository;

        private readonly ExecutionLogService _logRepository;
        private readonly OneTimeJobScheduler _oneTimeJobService;
        private readonly JobMonitoringService _jobStatusService;
        
        private readonly ConfigStatusService _statusService;
        private readonly CombinedRefDataService _combinedRefDataService;
        private readonly SettingsRepository _settingsRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly ShopifyPaymentGatewayService _gatewayService;

        public ConfigController(
                CredentialsRepository connectionRepository,
                StateRepository stateRepository,
                ExecutionLogService logRepository,
                OneTimeJobScheduler oneTimeJobService,
                JobMonitoringService jobStatusService,
                ConfigStatusService statusService, 
                CombinedRefDataService combinedRefDataService,
                SettingsRepository settingsRepository, 
                SyncInventoryRepository syncInventoryRepository, 
                ShopifyPaymentGatewayService gatewayService)
        {
            _connectionRepository = connectionRepository;
            _stateRepository = stateRepository;
            _oneTimeJobService = oneTimeJobService;

            _statusService = statusService;
            _combinedRefDataService = combinedRefDataService;
            _settingsRepository = settingsRepository;
            _logRepository = logRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _gatewayService = gatewayService;
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
            return new JsonNetResult(status);
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
                        model.InstanceUrl, model.Branch, model.Company, model.UserName, model.Password);
            }

            _oneTimeJobService.ConnectToAcumatica();
            return JsonNetResult.Success();
        }
        

        // Acumatica Reference Data Pull
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
            return new JsonNetResult(status);
        }
        

        // Settings data entry
        //
        [HttpGet]
        public ActionResult Settings()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AcumaticaReferenceData()
        {
            var data = _combinedRefDataService.RetrieveRefData();
            return new JsonNetResult(data);
        }

        [HttpGet]
        public ActionResult SettingsSelections()
        {
            var output = new SettingsSelectionsModel();

            var settings = _settingsRepository.RetrieveSettings();
            output.AcumaticaTimeZone = settings.AcumaticaTimeZone;
            output.AcumaticaDefaultItemClass = settings.AcumaticaDefaultItemClass;
            output.AcumaticaDefaultPostingClass = settings.AcumaticaDefaultPostingClass;
            output.AcumaticaDefaultCustomerClass = settings.AcumaticaCustomerClass;

            var selectedGateways = _settingsRepository.RetrievePaymentGateways();

            foreach (var selectedGateway in selectedGateways)
            {
                var gateway = new PaymentGatewaySelectionModel();
                gateway.ShopifyGatewayId = selectedGateway.ShopifyGatewayId;
                gateway.ShopifyGatewayName = _gatewayService.Name(selectedGateway.ShopifyGatewayId);
                gateway.AcumaticaPaymentMethod = selectedGateway.AcumaticaPaymentMethod;
                gateway.AcumaticaCashAcount = selectedGateway.AcumaticaCashAccount;

                output.PaymentGateways.Add(gateway);
            }

            var selectedCarriers = _settingsRepository.RetrieveRateToShipVias();
            
            foreach (var selectedCarrier in selectedCarriers)
            {
                var rate = new CarrierToShipViaSelection();
                rate.ShopifyRateName = selectedCarrier.ShopifyRateName;
                rate.AcumaticaCarrierId = selectedCarrier.AcumaticaShipViaId;

                output.RateToShipVias.Add(rate);
            }

            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult SettingsSelections(SettingsSelectionsModel selectionsModel)
        {
            var data = _settingsRepository.RetrieveSettings();
            
            // Save Settings
            //
            data.AcumaticaTimeZone = selectionsModel.AcumaticaTimeZone;
            data.AcumaticaDefaultItemClass = selectionsModel.AcumaticaDefaultItemClass;
            data.AcumaticaDefaultPostingClass = selectionsModel.AcumaticaDefaultPostingClass;
            data.AcumaticaCustomerClass = selectionsModel.AcumaticaDefaultCustomerClass;

            _settingsRepository.SaveChanges();

            // Save Gateways
            //
            var gatewayRecords =
                selectionsModel.PaymentGateways.Select(x => new PaymentGateway()
                {
                    ShopifyGatewayId = x.ShopifyGatewayId,
                    AcumaticaCashAccount = x.AcumaticaCashAcount,
                    AcumaticaPaymentMethod = x.AcumaticaPaymentMethod,
                })
                .ToList();
            _settingsRepository.ImprintPaymentGateways(gatewayRecords);

            // Save Carrier-to-Ship-Via mappings
            //
            var carrierToShipVia =
                selectionsModel.RateToShipVias.Select(x => new RateToShipVia()
                {
                    ShopifyRateName = x.ShopifyRateName,
                    AcumaticaShipViaId = x.AcumaticaCarrierId,
                })
                .ToList();
            _settingsRepository.ImprintRateToShipVias(carrierToShipVia);

            // Refresh the Settings Status
            //
            _statusService.RefreshSettingsStatus();

            return JsonNetResult.Success();
        }


        // Settings -> Taxes
        //
        [HttpGet]
        public ActionResult SettingsTaxes()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SettingsTaxesSelections()
        {
            var settings = _settingsRepository.RetrieveSettings();

            var output = new SettingsTaxesModel();
            output.AcumaticaTaxZone = settings.AcumaticaTaxZone;
            output.AcumaticaTaxableCategory = settings.AcumaticaTaxableCategory;
            output.AcumaticaTaxExemptCategory = settings.AcumaticaTaxExemptCategory;
            output.AcumaticaLineItemTaxId = settings.AcumaticaLineItemTaxId;
            output.AcumaticaFreightTaxId = settings.AcumaticaFreightTaxId;

            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult SettingsTaxesSelections(SettingsTaxesModel model)
        {
            var settings = _settingsRepository.RetrieveSettings();

            settings.AcumaticaTaxZone = model.AcumaticaTaxZone;
            settings.AcumaticaTaxableCategory = model.AcumaticaTaxableCategory;
            settings.AcumaticaTaxExemptCategory = model.AcumaticaTaxExemptCategory;
            settings.AcumaticaLineItemTaxId = model.AcumaticaLineItemTaxId;
            settings.AcumaticaFreightTaxId = model.AcumaticaFreightTaxId;
            _settingsRepository.SaveChanges();

            _statusService.RefreshSettingsTaxesStatus();
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
            var state = _stateRepository.RetrieveSystemStateNoTracking();
            var details = _statusService.GetWarehouseSyncStatus();

            var output = new WarehouseSyncStatusModel()
            {
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
            return Redirect(GlobalConfig.Url("/Config/CompleteScreen"));
        }

        [HttpGet]
        public ActionResult CompleteScreen()
        {
            return View();
        }
    }
}

