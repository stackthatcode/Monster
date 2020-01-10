using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Monster.Middle.Persist.Master;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Identity;

namespace Monster.Middle.Identity
{
    public class ProvisioningService
    {
        private readonly MasterRepository _masterRepository;
        private readonly PushIdentityDbContext _dbContext;
        private readonly IdentityUserManager _userManager;
        private readonly IdentityRoleManager _roleManager;
        private readonly IPushLogger _logger;

        public ProvisioningService(
                MasterRepository masterRepository,
                PushIdentityDbContext dbContext,
                IdentityUserManager userManager,
                IdentityRoleManager roleManager,
                IPushLogger logger)
        {
            _masterRepository = masterRepository;
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }


        public void PopulateRolesAndAdmin()
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

            return user;
        }

        public List<ApplicationUser> RetrieveUsers()
        {
            var output = _dbContext.Users.Include(x => x.Logins).Include(x => x.Roles).ToList();

                //.Where(x => x.Roles.Any(y => y.RoleId == role))
            return output;
        }

        public void AssignNextAvailableInstance(string email, string domain, string nickname)
        {
            var instance = _masterRepository.RetrieveNextAvailableInstance();
            if (instance == null)
            {
                throw new Exception("There are no Instances available for assignment");
            }

            var user = _userManager.FindByName(email);
            if (user == null)
            {
                throw new Exception($"Unable to locate User {email}");
            }

            var login = user.Logins.Where(x => x.LoginProvider == "Shopify" && x.ProviderKey == domain).ToList();
            if (login == null)
            {
                throw new Exception($"Unable to locate Domain {domain} for User {email}");
            }

            var domainInstance = _masterRepository.RetrieveInstanceByDomain(domain);
            if (domainInstance != null)
            {
                throw new Exception($"Instance {domainInstance.Id} already assigned to domain {domainInstance.OwnerDomain}");
            }

            _masterRepository.AssignInstanceToUser(instance.Id, user.Id, nickname, domain);
            _logger.Info($"Assigned User {user.Email} to Instance {instance.Id}");
        }

        public void RevokeInstanceByDomain(string domain)
        {
            var instance = _masterRepository.RetrieveInstanceByDomain(domain);
            if (domain == null)
            {
                throw new Exception($"Unable to locate Domain {domain} for revocation");
            }

            _masterRepository.RevokeInstance(instance.Id);
        }

        public string RetrieveUserIdByShopifyDomain(string domain)
        {
            var userId = _dbContext
                .Users
                .Where(x => x.Logins.Any(y => y.ProviderKey == domain))
                .Select(x => x.Id)
                .FirstOrDefault();
            return userId;
        }

    }
}
