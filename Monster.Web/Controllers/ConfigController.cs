using System.Web.Mvc;
using AutoMapper;
using Monster.Middle.Persist.Multitenant;
using Monster.Web.Attributes;
using Monster.Web.Models;

namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class ConfigController : Controller
    {
        private readonly TenantRepository _tenantRepository;

        public ConfigController(TenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }


        public ActionResult Home()
        {
            return View();
        }

        public ActionResult Preferences()
        {
            var preferencesData = _tenantRepository.RetrievePreferences();
            var model = Mapper.Map<Preferences>(preferencesData);
            return View(model);
        }

        public ActionResult Warehouses()
        {
            return View();
        }

        public ActionResult Inventory()
        {
            return View();
        }
    }
}

