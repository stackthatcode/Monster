using System.Web.Mvc;
using AutoMapper;
using Hangfire;
using Monster.Middle.Directors;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Model;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Web.Attributes;
using Monster.Web.Models;
using Push.Foundation.Web.Json;

namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class ConfigController : Controller
    {
        private readonly TenantRepository _tenantRepository;
        private readonly QueuingService _queuingService;
        private readonly InventoryStatusService _inventoryStatusService;

        public ConfigController(
                TenantRepository tenantRepository, 
                QueuingService queuingService, 
                InventoryStatusService inventoryStatusService)
        {
            _tenantRepository = tenantRepository;
            _queuingService = queuingService;
            _inventoryStatusService = inventoryStatusService;
        }


        [HttpGet]

        public ActionResult Home()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Preferences()
        {
            var preferencesData = _tenantRepository.RetrievePreferences();
            var model = Mapper.Map<Preferences>(preferencesData);
            return View(model);
        }

        [HttpGet]
        public ActionResult Warehouses()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Inventory()
        {
            return View();
        }

        public ActionResult RealTime()
        {
            return View();
        }



        // AJAX methods
        [HttpPost]
        public ActionResult SyncWarehouses()
        {
            _queuingService.SyncWarehouseAndLocation();

            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult WarehouseSyncStatus()
        {
            var monitor = _tenantRepository.RetrieveJobMonitor();

            var output = new WarehouseSyncStatusModel()
            {
                JobStatus = monitor.WarehouseSyncStatus
            };

            if (monitor.WarehouseSyncStatus == JobStatus.Complete)
            {
                output.SyncState
                    = _inventoryStatusService.GetWarehouseSyncStatus();
            }
            
            return new JsonNetResult(output);
        }
    }
}

