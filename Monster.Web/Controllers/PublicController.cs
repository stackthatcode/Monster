using System.Web.Mvc;

namespace Monster.Web.Controllers
{
    public class PublicController : Controller
    {
        public ActionResult Splash()
        {
            return View();
        }
    }
}