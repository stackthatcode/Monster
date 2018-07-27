using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Push.Foundation.Web.Identity;

namespace Push.Foundation.Web.Helpers
{
    public static class ControllerHelpers
    {
        public static void AddErrors(this Controller controller, IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                controller.ModelState.AddModelError("", error);
            }
        }

        public static string ExtractUserId(this IPrincipal user)
        {
            var identity = user.Identity as ClaimsIdentity;
            if (identity?.GetUserId() != null)
            {
                return identity.GetUserId();
            }
            else
            {
                return null;
            }
        }

        // TODO - Need to fix this and retest everything
        public static string ExtractUserId(this HttpContext context)
        {
            var claimsPrincipal = HttpContext.Current.User as ClaimsPrincipal;
            return claimsPrincipal?.ExtractUserId();
        }

        // TODO - Need to fix this and retest everything
        public static string ExtractUserId(this HttpContextBase context)
        {
            var claimsPrincipal = HttpContext.Current.User as ClaimsPrincipal;
            return claimsPrincipal?.ExtractUserId();
        }


        public static string ExtractUserEmail(this IPrincipal user)
        {
            var identity = user.Identity as ClaimsIdentity;
            return identity.Claims
                        .FirstOrDefault(x => x.Type == ApplicationUser.EmailAddressClaim)?
                        .Value;
        }

        public static string ExtractUserEmail(this HttpContextBase context)
        {
            var claimsPrincipal = HttpContext.Current.User as ClaimsPrincipal;
            return claimsPrincipal?.ExtractUserEmail();
        }

        public static string ExtractUserEmail(this HttpContext context)
        {
            var claimsPrincipal = HttpContext.Current.User as ClaimsPrincipal;
            return claimsPrincipal?.ExtractUserEmail();
        }
    }
}

