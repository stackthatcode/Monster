using System;
using System.Web.Mvc;
using Monster.Middle.Security;
using Monster.Middle.Services;
using Monster.Web.Attributes;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Attributes
{
    public class IdentityProcessorAttribute : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            var logger = DependencyResolver.Current.GetService<IPushLogger>();
            var tenantContext = DependencyResolver.Current.GetService<ConnectionContext>();

            //var userId = filterContext.HttpContext.User.ExtractUserId();
            //if (userId != null)

            // Step 1 - hydrate Identity
            // TODO - replace with mapping between Foundation and MonsterSys
            var identity = new IdentityContext();
            identity.AspNetUserId = "6413ab76-8410-4524-81f0-2903383476dd";
            identity.Email = "aleksjones@gmail.com";
            identity.AspNetRoles.Add(SecurityConfig.AdminRole);
            identity.InstallationId = Guid.Parse("51AA413D-E679-4F38-BA47-68129B3F9212");
            identity.NickName = "onemoreteststorecanthurt";

            filterContext.HttpContext.SetIdentity(identity);
            logger.Debug($"Successfully hydrated Identity {identity.AspNetUserId} / {identity.Email}");

            // Step 2 - hydrate Tenant Context
            tenantContext.InitializePersistOnly(identity.InstallationId);
        }
    }
}
