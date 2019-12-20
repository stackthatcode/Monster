using System;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Monster.Middle.Misc.State;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Master;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Identity;

namespace Monster.Middle.Identity
{
    public class IdentityService
    {
        private readonly MasterRepository _masterRepository;
        private readonly PushIdentityDbContext _dbContext;
        private readonly IdentityUserManager _userManager;
        private readonly IdentityRoleManager _roleManager;
        private readonly IdentitySignInManager _signInManager;

        private readonly InstanceContext _instanceContext;
        private readonly StateRepository _stateRepository;

        private readonly IPushLogger _logger;

        public IdentityService(
                MasterRepository masterRepository,
                PushIdentityDbContext dbContext,
                IdentityUserManager userManager,
                IdentityRoleManager roleManager,
                IdentitySignInManager signInManager,
                InstanceContext instanceContext,
                StateRepository stateRepository,
                IPushLogger logger)
        {
            _masterRepository = masterRepository;
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _instanceContext = instanceContext;
            _stateRepository = stateRepository;
            _logger = logger;
        }

        public IdentityContext HydrateIdentity(string userId)
        {
            if (userId.IsNullOrEmpty())
            {
                return new IdentityContext();
            }

            // If the User's instance is not enabled, vacate
            //
            var instances = _masterRepository.RetrieveInstanceByUserId(userId);
            if (instances.Count > 1)
            {
                // *** Pending multi-domain feature release
                //
                throw new NotImplementedException("Multiple Instances feature not available yet");
            }

            var instance = instances.First();
            if (!instance.IsEnabled)
            {
                SignOut();
                return new IdentityContext();
            }

            // Successfully located User
            //
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == userId);
            var userRoleIds = user.Roles.Select(x => x.RoleId);
            var userRoles =
                _roleManager.Roles
                    .Where(x => userRoleIds.Contains(x.Id))
                    .Select(x => x.Name)
                    .ToList();

            var context = new IdentityContext();
            context.AspNetUserId = user.Id;
            context.Email = user.Email;
            context.AspNetRoles = userRoles;

            // Located valid Instance - load data into properties
            //
            context.InstanceId = instance.Id;
            context.InstanceNickName = instance.OwnerNickname;

            // Auto-initialize persistence layer
            //
            _instanceContext.InitializePersistOnly(instance.Id);

            var state = _stateRepository.RetrieveSystemStateNoTracking();
            context.SystemState = state;
            return context;
        }

        public void SignInAspNetUser(string aspNetUserId)
        {
            var user = _userManager.Users.First(x => x.Id == aspNetUserId);
            _signInManager.SignIn(user, true, true);
        }

        public void SignOut()
        {
            _signInManager.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            _signInManager.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}
