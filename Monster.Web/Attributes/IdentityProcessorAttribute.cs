using System;
using System.Web.Mvc;
using Monster.Middle.Identity;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Logging;

namespace Monster.Web.Attributes
{
    public class IdentityProcessorAttribute : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            var logger = DependencyResolver.Current.GetService<IPushLogger>();
            
            // Step 1 - hydrate Identity
            // TODO - replace with mapping between Foundation and MonsterSys
            var identity = new IdentityContext();
            identity.AspNetUserId = "6413ab76-8410-4524-81f0-2903383476dd";
            identity.Email = "aleksjones@gmail.com";
            identity.AspNetRoles.Add(SecurityConfig.UserRole);
            identity.InstanceId = Guid.Parse("51AA413D-E679-4F38-BA47-68129B3F9212");
            identity.InstanceNickName = "onemoreteststorecanthurt";
            
            logger.Debug($"Successfully hydrated Identity {identity.AspNetUserId} / {identity.Email}");

            // Step 2 - hydrate State
            var connectionContext = DependencyResolver.Current.GetService<ConnectionContext>();
            var stateRepository = DependencyResolver.Current.GetService<StateRepository>();

            connectionContext.Initialize(identity.InstanceId);
            var state = stateRepository.RetrieveSystemStateNoTracking();
            identity.IsRealTimeMode = state.IsRandomAccessMode;


            // FIN
            filterContext.HttpContext.SetIdentity(identity);
        }
    }
}
