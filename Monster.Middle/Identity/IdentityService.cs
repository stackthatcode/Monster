﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Monster.Middle.Misc.External;
using Monster.Middle.Misc.State;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Master;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Identity;

namespace Monster.Middle.Identity
{
    public class IdentityService
    {
        private readonly MasterRepository _systemRepository;
        private readonly PushIdentityDbContext _dbContext;
        private readonly IdentityUserManager _userManager;
        private readonly IdentityRoleManager _roleManager;
        private readonly IdentitySignInManager _signInManager;

        private readonly InstanceContext _instanceContext;
        private readonly StateRepository _stateRepository;

        private readonly IPushLogger _logger;

        public IdentityService(
                MasterRepository systemRepository,
                PushIdentityDbContext dbContext,
                IdentityUserManager userManager,
                IdentityRoleManager roleManager,
                IdentitySignInManager signInManager,
                InstanceContext instanceContext,
                StateRepository stateRepository,
                IPushLogger logger)
        {
            _systemRepository = systemRepository;
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _instanceContext = instanceContext;
            _stateRepository = stateRepository;
            _logger = logger;
        }

        public  void PopulateRolesAndAdmin()
        {
            // Check to see if Role Exists, if not create it
            if (!_roleManager.RoleExists(SecurityConfig.AdminRole))
            {
                _logger.Info($"Role {SecurityConfig.AdminRole} does not exist - adding to Roles");
                var result = _roleManager.Create(new IdentityRole(SecurityConfig.AdminRole));
                if (result.Succeeded == false)
                {
                    throw new Exception($"RoleManager.Create (Admin) failed: {result.Errors.JoinByNewline()}");
                }
            }

            if (!_roleManager.RoleExists(SecurityConfig.UserRole))
            {
                _logger.Info($"Role {SecurityConfig.UserRole} does not exist - adding to Roles");
                var result = _roleManager.Create(new IdentityRole(SecurityConfig.UserRole));
                if (result.Succeeded == false)
                {
                    throw new Exception($"RoleManager.Create (User) failed: {result.Errors.JoinByNewline()}");
                }
            }

            var adminUser = _userManager.FindByName(SecurityConfig.DefaultAdminEmail);
            if (adminUser == null)
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    _logger.Info(
                        $"Unable to locate default Sys Admin: {SecurityConfig.DefaultAdminEmail} - "
                        + @"creating new Sys Admin");

                    var newAdminUser = new ApplicationUser()
                    {
                        UserName = SecurityConfig.DefaultAdminEmail,
                        Email = SecurityConfig.DefaultAdminEmail,
                    };

                    var result = _userManager.Create(newAdminUser, SecurityConfig.DefaultAdminPassword);
                    if (result.Succeeded == false)
                    {
                        throw new Exception(
                            $"UserManager.Create failed: {result.Errors.JoinByNewline()}");
                    }

                    var resultAddToAdmin = _userManager.AddToRole(newAdminUser.Id, SecurityConfig.AdminRole);
                    if (resultAddToAdmin.Succeeded == false)
                    {
                        throw new Exception(
                            $"UserManager.AddToRole (Admin) failed: {resultAddToAdmin.Errors.JoinByNewline()}");
                    }

                    transaction.Commit();
                }
            }
        }

        public async Task<ApplicationUser> ProvisionNewAccount(string emailAddress, string domain)
        {
            var user = new ApplicationUser()
            {
                Email = emailAddress,
                UserName = emailAddress,
            };

            using (var transaction = _dbContext.Database.BeginTransaction())
            {

                var createUserResult = await _userManager.CreateAsync(user);
                if (!createUserResult.Succeeded)
                {
                    _logger.Error(
                        $"Unable to create new User for {user.Email}/{user.UserName} - " +
                        $"{createUserResult.Errors.ToCommaDelimited()}");
                    return null;
                }

                _logger.Info($"Created new User {user.Email}");

                var addToRoleResult = await _userManager.AddToRoleAsync(user.Id, SecurityConfig.UserRole);
                if (!addToRoleResult.Succeeded)
                {
                    _logger.Error(
                        $"Unable to add User {user.Email}/{user.UserName} to {SecurityConfig.UserRole} - " +
                        $"{createUserResult.Errors.ToCommaDelimited()}");
                    return null;
                }

                _logger.Info($"Added User {user.Email} to Role {SecurityConfig.UserRole}");

                var login = new UserLoginInfo("Shopify", domain);

                var addLoginResult = await _userManager.AddLoginAsync(user.Id, login);
                if (!addLoginResult.Succeeded)
                {
                    _logger.Error(
                        $"Unable to add Login for User {user.Email}/{user.UserName} - " +
                        $"{addLoginResult.Errors.StringJoin(";")}");
                    return null;
                }

                _logger.Info($"Added User {user.Email} Login {login.LoginProvider} / {login.ProviderKey}");

                transaction.Commit();
            }

            return AssignNextAvailableInstanceToUser(emailAddress);
        }

        public ApplicationUser AssignNextAvailableInstanceToUser(string userEmail)
        {
            var user = _userManager.FindByName(userEmail);
            if (user == null)
            {
                _logger.Error($"Unable to locate User {userEmail}");
                return null;
            }

            var login = user.Logins.First(x => x.LoginProvider == "Shopify");
            var domain = login.ProviderKey;

            var instance = _systemRepository.RetrieveNextAvailableInstance();
            if (instance == null)
            {
                _logger.Error("There are no Instances available for assignment");
                return null;
            }

            _systemRepository.AssignInstanceToUser(instance.Id, user.Id, domain);
            _logger.Info($"Assigned User {user.Email} to Instance {instance.Id}");
            return user;
        }

        public IdentityContext ProcessIdentityByDomain(string domain)
        {
            var userId = _dbContext
                .Users
                .Where(x => x.Logins.Any(y => y.ProviderKey == domain))
                .Select(x => x.Id)
                .FirstOrDefault();

            return ProcessIdentity(userId);
        }

        public IdentityContext ProcessIdentity(string userId)
        {
            if (userId.IsNullOrEmpty())
            {
                return new IdentityContext();
            }

            var context = new IdentityContext();

            // ASP.NET User Identity
            //
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == userId);
            var instance = _systemRepository.RetrieveInstanceByUserId(userId);

            // Block disabled accounts from authenticating
            //
            if (!instance.IsEnabled)
            {
                SignOut();
                return context;
            }

            // Successfully located User
            //
            var userRoleIds = user.Roles.Select(x => x.RoleId);
            var userRoles =
                _roleManager.Roles
                    .Where(x => userRoleIds.Contains(x.Id))
                    .Select(x => x.Name)
                    .ToList();

            context.AspNetUserId = user.Id;
            context.Email = user.Email;
            context.AspNetRoles = userRoles;

            // System -> Instance
            //
            context.InstanceId = instance.Id;
            context.InstanceNickName = instance.OwnerNickname;

            // Instance -> State
            //
            _instanceContext.InitializePersistOnly(instance.Id);

            var state = _stateRepository.RetrieveSystemStateNoTracking();
            context.SystemState = state;
            return context;
        }

        public void SignInAspNetUser(string identityAspNetUserId)
        {
            var user = _userManager.Users.First(x => x.Id == identityAspNetUserId);
            _signInManager.SignIn(user, true, true);
        }

        public void SignOut()
        {
            _signInManager.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            _signInManager.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}
