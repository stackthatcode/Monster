using System;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Identity;

namespace Monster.Middle.Identity
{
    public class IdentityService
    {
        private readonly PushIdentityDbContext _dbContext;
        private readonly IdentityUserManager _userManager;
        private readonly IdentityRoleManager _roleManager;
        private readonly IPushLogger _logger;

        public IdentityService(
                PushIdentityDbContext dbContext,
                IdentityUserManager userManager,
                IdentityRoleManager roleManager,
                IPushLogger logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async void HydrateRolesAndAdmin()
        {
            // Check to see if Role Exists, if not create it
            if (!await _roleManager.RoleExistsAsync(SecurityConfig.AdminRole))
            {
                _logger.Info($"Role {SecurityConfig.AdminRole} does not exist - adding to Roles");
                var result = _roleManager.Create(new IdentityRole(SecurityConfig.AdminRole));
                if (result.Succeeded == false)
                {
                    throw new Exception($"RoleManager.Create (Admin) failed: {result.Errors.JoinByNewline()}");
                }
            }

            if (!await _roleManager.RoleExistsAsync(SecurityConfig.UserRole))
            {
                _logger.Info($"Role {SecurityConfig.UserRole} does not exist - adding to Roles");
                var result = _roleManager.Create(new IdentityRole(SecurityConfig.UserRole));
                if (result.Succeeded == false)
                {
                    throw new Exception($"RoleManager.Create (User) failed: {result.Errors.JoinByNewline()}");
                }
            }

            var adminUser = await _roleManager.FindByNameAsync(SecurityConfig.DefaultAdminEmail);
            if (adminUser == null)
            {
                _logger.Info(
                    $"Unable to locate default Sys Admin: {SecurityConfig.DefaultAdminEmail} - "
                    + @"creating new Sys Admin");

                var newAdminUser = new ApplicationUser()
                {
                    UserName = SecurityConfig.DefaultAdminEmail,
                    Email = SecurityConfig.DefaultAdminEmail,
                };

                var result = await _userManager.CreateAsync(newAdminUser, SecurityConfig.DefaultAdminPassword);
                if (result.Succeeded == false)
                {
                    throw new Exception(
                        $"UserManager.Create failed: {result.Errors.JoinByNewline()}");
                }

                var resultAddToAdmin = await _userManager.AddToRoleAsync(newAdminUser.Id, SecurityConfig.AdminRole);
                if (resultAddToAdmin.Succeeded == false)
                {
                    throw new Exception(
                        $"UserManager.AddToRole (Admin) failed: {resultAddToAdmin.Errors.JoinByNewline()}");
                }

                //var resultAddToUser = userManager.AddToRole(newAdminUser.Id, SecurityConfig.UserRole);
                //if (resultAddToUser.Succeeded == false)
                //{
                //    throw new Exception(
                //        $"UserManager.AddToRole (User) failed: {resultAddToUser.Errors.JoinByNewline()}");
                //}
            }
        }

        public async Task<ApplicationUser> CreateNewAccount(string emailAddress, string shopifyUrl)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                var user = new ApplicationUser()
                {
                    Email = emailAddress,
                    UserName = emailAddress,
                };

                var createUserResult = await _userManager.CreateAsync(user);
                if (!createUserResult.Succeeded)
                {
                    _logger.Error(
                        $"Unable to create new User for {user.Email}/{user.UserName} - " +
                        $"{createUserResult.Errors.ToCommaDelimited()}");
                    return null;
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(user.Id, SecurityConfig.UserRole);
                if (!addToRoleResult.Succeeded)
                {
                    _logger.Error(
                        $"Unable to add User {user.Email}/{user.UserName} to {SecurityConfig.UserRole} - " +
                        $"{createUserResult.Errors.ToCommaDelimited()}");
                    return null;
                }

                var login = new UserLoginInfo("Shopify", shopifyUrl);

                var addLoginResult = await _userManager.AddLoginAsync(user.Id, login);
                if (!addLoginResult.Succeeded)
                {
                    _logger.Error(
                        $"Unable to add Login for User {user.Email}/{user.UserName} - " +
                        $"{addLoginResult.Errors.StringJoin(";")}");
                    return null;
                }

                transaction.Commit();
                return user;
            }
        }
    }
}
