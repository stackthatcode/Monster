using System;
using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Sys;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;

namespace Monster.Middle.Services
{
    public class TenantContext
    {
        private readonly SystemRepository _accountRepository;
        private readonly TenantDataRepository _tenantContextRepository;
        private readonly PersistContext _persistContext;
        private readonly ShopifyHttpContext _shopifyHttpContext;
        private readonly AcumaticaHttpContext _acumaticaHttpContext;


        public TenantContext(
                TenantDataRepository tenantContextRepository, 
                SystemRepository accountRepository, 
                PersistContext persistContext, 
                ShopifyHttpContext shopifyHttpContext,
                AcumaticaHttpContext acumaticaHttpContext)
        {
            _tenantContextRepository = tenantContextRepository;
            _accountRepository = accountRepository;
            _persistContext = persistContext;
            _shopifyHttpContext = shopifyHttpContext;
            _acumaticaHttpContext = acumaticaHttpContext;
        }

        public void Initialize(Guid installationId)
        {
            var installation = 
                    _accountRepository
                        .RetrieveInstallation(installationId);

            // Load the Tenant into Persist 
            _persistContext.Initialize(
                    installation.ConnectionString, installation.CompanyId);

            // Shopify
            var shopifyCredentials
                    = _tenantContextRepository.RetrieveShopifyCredentials();
            _shopifyHttpContext.Initialize(shopifyCredentials);

            // Acumatica
            var acumaticaCredentials
                    = _tenantContextRepository.RetrieveAcumaticaCredentials();
            _acumaticaHttpContext.Initialize(acumaticaCredentials);
        }

        public void InitializePersistOnly(Guid installationId)
        {
            var installation = 
                    _accountRepository.RetrieveInstallation(installationId);

            // Load the Tenant into Persist 
            _persistContext.Initialize(
                installation.ConnectionString, installation.CompanyId);

        }

        public void Initialize(
                string connectionString, 
                long companyId,
                IShopifyCredentials shopifyCredentials,
                AcumaticaCredentials acumaticaCredentials)
        {
            // Load the Tenant into Persist 
            _persistContext.Initialize(connectionString, companyId);

            // Shopify
            _shopifyHttpContext.Initialize(shopifyCredentials);

            // Acumatica
            _acumaticaHttpContext.Initialize(acumaticaCredentials);
        }
    }
}
