using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace Push.Foundation.Web.Identity
{

    // Configure the application sign-in manager which is used in this application.
    public class IdentitySignInManager : SignInManager<ApplicationUser, string>
    {
        public IdentitySignInManager(IdentityUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }
    }

}
