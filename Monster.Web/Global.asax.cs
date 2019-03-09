using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Monster.Web.Controllers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;

namespace Monster.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        

        protected void Application_Error(object sender, EventArgs e)
        {
            // Explicitly instantiate dependencies
            var logger = DependencyResolver.Current.GetService<IPushLogger>();
            IController errorController = DependencyResolver.Current.GetService<ErrorController>();

            var lastError = Server.GetLastError();

            // We log everything except for HTTP 404's
            if (!lastError.IsHttpExceptionWithCode(404))
            {
                logger.Error(lastError);
            }

            // Build the route based on error type
            var errorRoute = new RouteData();
            var context = new HttpContextWrapper(HttpContext.Current);

            errorRoute.Values.Add("controller", "Error");

            if (lastError.IsHttpExceptionWithCode(404))
            {
                errorRoute.Values.Add("action", "Http404");

                // Strangely enough, without this AJAX requests are not getting their
                // ... Status Code properly set to an HTTP 404
                context.Response.StatusCode = 404;
            }
            else if (lastError.IsHttpExceptionWithCode(403))
            {
                errorRoute.Values.Add("action", "Http403");
            }
            else
            {
                errorRoute.Values.Add("action", "Http500");
            }

            errorRoute.Values.Add("returnUrl", HttpContext.Current.Request.Url.OriginalString);

            // Clear the error on Server and the Reponse
            Server.ClearError();
            Response.Clear();
            
            errorController.Execute(new RequestContext(context, errorRoute));
        }
    }
}
