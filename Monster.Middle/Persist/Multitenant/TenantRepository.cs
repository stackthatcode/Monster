﻿using System.Data.Entity;
using System.Linq;
using Monster.Acumatica.Http;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Security;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;

namespace Monster.Middle.Persist.Multitenant
{
    public class TenantRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        private readonly ICryptoService _cryptoService;

        public TenantRepository(
                PersistContext dataContext,
                ICryptoService cryptoService)
        {
            _dataContext = dataContext;
            _cryptoService = cryptoService;
        }

        public DbContextTransaction BeginTransaction()
        {
            return Entities.Database.BeginTransaction();
        }

        // TODO => implement this https://stackoverflow.com/questions/202011/encrypt-and-decrypt-a-string-in-c/10366194#10366194


        // Tenant Context
        //
        public void CreateIfMissingContext()
        {
            if (!Entities.UsrTenants.Any())
            {
                this.InsertTenant(new UsrTenant()
                {
                    CompanyId = _dataContext.CompanyId
                });
            }
        }

        public void InsertTenant(UsrTenant context)
        {
            Entities.UsrTenants.Add(context);
            Entities.SaveChanges();
        }
        
        public UsrTenant Retrieve()
        {
            return Entities.UsrTenants.FirstOrDefault();
        }


        // Shopify Credentials
        //
        public IShopifyCredentials RetrieveShopifyCredentials()
        {
            var tenant = Retrieve();
            
            var accessToken =
                tenant.ShopifyApiKey.IsNullOrEmpty()
                    ? "" : _cryptoService.Decrypt(tenant.ShopifyAccessToken);
            
            var apiKey =
                tenant.ShopifyApiKey.IsNullOrEmpty() 
                    ? "" : _cryptoService.Decrypt(tenant.ShopifyApiKey);

            var apiPassword = 
                tenant.ShopifyApiPassword.IsNullOrEmpty()
                    ? "" : _cryptoService.Decrypt(tenant.ShopifyApiPassword);

            var domain = new ShopDomain(tenant.ShopifyDomain);

            if (accessToken.IsNullOrEmpty())
            {
                return new PrivateAppCredentials(apiKey, apiPassword, domain);
            }
            else
            {
                return new OAuthAccessToken(tenant.ShopifyDomain, accessToken);
            }
        }
        
        public void UpdateShopifyCredentials(
                string shopifyPrivateAppDomain,
                string shopifyApiKey,
                string shopifyApiPassword,
                string shopifyApiSecret)
        {
            var context = Retrieve();

            var encryptedKey = _cryptoService.Encrypt(shopifyApiKey);
            var encryptedPassword = _cryptoService.Encrypt(shopifyApiPassword);
            var encryptedSecret = _cryptoService.Encrypt(shopifyApiSecret);

            context.ShopifyDomain = shopifyPrivateAppDomain;
            context.ShopifyApiKey = encryptedKey;
            context.ShopifyApiPassword = encryptedPassword;
            context.ShopifyApiSecret = encryptedSecret;

            Entities.SaveChanges();
        }

        public void UpdateShopifyCredentials(string shopifyAccessToken)
        {
            var context = Retrieve();
            var encryptedAccessToken = _cryptoService.Encrypt(shopifyAccessToken);
            context.ShopifyAccessToken = encryptedAccessToken;
            Entities.SaveChanges();
        }



        // Acumatica Credentials
        //
        public AcumaticaCredentials RetrieveAcumaticaCredentials()
        {
            var context = Retrieve();

            var output = new AcumaticaCredentials();
            output.InstanceUrl = context.AcumaticaInstanceUrl;
            output.Branch = context.AcumaticaBranch;
            output.CompanyName = context.AcumaticaCompanyName;

            output.Username =
                context.AcumaticaUsername.IsNullOrEmpty()
                    ? "" : _cryptoService.Decrypt(context.AcumaticaUsername);

            output.Password =
                context.AcumaticaPassword.IsNullOrEmpty()
                    ? "" : _cryptoService.Decrypt(context.AcumaticaPassword);

            return output;
        }

        public void UpdateAcumaticaCredentials(
            string acumaticaInstanceUrl,
            string acumaticaBranch,
            string acumaticaCompanyName,
            string acumaticaUsername,
            string acumaticaPassword)
        {
            var context = Retrieve();

            var encryptedUsername = _cryptoService.Encrypt(acumaticaUsername);
            var encryptedPassword = _cryptoService.Encrypt(acumaticaPassword);

            context.AcumaticaInstanceUrl = acumaticaInstanceUrl;
            context.AcumaticaBranch = acumaticaBranch;
            context.AcumaticaCompanyName = acumaticaCompanyName;
            context.AcumaticaUsername = encryptedUsername;
            context.AcumaticaPassword = encryptedPassword;

            this.Entities.SaveChanges();
        }
        

        public UsrPreference RetrievePreferences()
        {
            return Entities.UsrPreferences.First();
        }
    }
}
