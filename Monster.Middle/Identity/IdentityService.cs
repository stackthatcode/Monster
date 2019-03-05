using System.Threading.Tasks;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Identity;

namespace Monster.Middle.Identity
{
    public class IdentityService
    {
        private readonly IdentityUserManager _userManager;
        private readonly IPushLogger _logger;

        public IdentityService(
                IdentityUserManager userManager, 
                IPushLogger logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ApplicationUser> 
                CreateNewAccount(string emailAddress, string password)
        {
            var user = new ApplicationUser()
            {
                Email = emailAddress,
                UserName = emailAddress,
            };

            var createUserResult = await _userManager.CreateAsync(user, password);
            if (!createUserResult.Succeeded)
            {
                _logger.Error(
                    $"Unable to create new User for {user.Email}/{user.UserName} - " +
                    $"{createUserResult.Errors.StringJoin(";")}");
                return null;
            }

            return user;
        }

    }
}
