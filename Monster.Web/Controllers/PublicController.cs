using System.Web.Mvc;
using Monster.Web.Attributes;

namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class PublicController : Controller
    {
        public ActionResult Splash()
        {
            return View();
        }
    }
}
