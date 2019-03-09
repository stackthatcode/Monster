using System;
using System.Net;
using System.Web.Mvc;
using Monster.Middle.Attributes;
using Monster.Web.Attributes;

namespace Monster.Web.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        [IdentityProcessor]
        [AllowAnonymous]
        public ActionResult Tester()
        {
            return View();
        }

        [HttpGet]
        [IdentityProcessor]
        [AllowAnonymous]
        public ActionResult ThrowsErrors()
        {
            throw new Exception("Exception thrown for error handler testing...");
        }
        

        [HttpGet]
        [IdentityProcessor]
        [AllowAnonymous]
        public ActionResult Http500(string returnUrl)
        {
            var model = new ErrorModel() { ReturnUrl = returnUrl };

            Response.ContentType = "text/html";
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;

            return View(model);
        }

        [HttpGet]
        [IdentityProcessor]
        [AllowAnonymous]
        public ActionResult Http403()
        {
            Response.ContentType = "text/html";
            Response.StatusCode = (int)HttpStatusCode.Forbidden;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;

            return View();
        }

        [HttpGet]
        [IdentityProcessor]
        [AllowAnonymous]
        public ActionResult Http404()
        {
            Response.ContentType = "text/html";
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;

            return View();
        }
    }
}