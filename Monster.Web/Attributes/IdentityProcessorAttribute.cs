using System.Web.Mvc;
using Monster.Middle.Identity;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;

namespace Monster.Web.Attributes
{
    public class IdentityProcessorAttribute : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            var logger = DependencyResolver.Current.GetService<IPushLogger>();
            var identityService = DependencyResolver.Current.GetService<IdentityService>();

            var userId = filterContext.HttpContext.User.ExtractUserId();
            var identity = identityService.HydrateIdentityContext(userId);

            filterContext.HttpContext.SetIdentity(identity);
            logger.Debug(
                $"Successfully hydrated Identity {identity.AspNetUserId} / {identity.Email}");
        }
    }
}
