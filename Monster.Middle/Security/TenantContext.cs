using System;
using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Sys.Repositories;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;

namespace Monster.Middle.Security
{
    public class TenantContext
    {
        private readonly SystemRepository _accountRepository;
        private readonly TenantRepository _tenantContextRepository;
        private readonly PersistContext _persistContext;
        private readonly ShopifyHttpContext _shopifyHttpContext;
        private readonly AcumaticaHttpContext _acumaticaHttpContext;
        
        // Why not add pass throughs Shopify and Acumatica...?
        // Lifetime Scope says we're safe
        private Guid? _tenantId;


        public TenantContext(
                TenantRepository tenantContextRepository, 
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

        public Guid InstallationId => _tenantId.Value;

        public void Initialize(Guid installationId)
        {
            _tenantId = installationId;

            var installation = 
                    _accountRepository
                        .RetrieveInstallation(installationId);

            // Load the Installation into Persist 
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
            _tenantId = installationId;

            var installation = 
                    _accountRepository.RetrieveInstallation(installationId);

            // Load the Installation into Persist 
            _persistContext.Initialize(
                installation.ConnectionString, installation.CompanyId);

        }        
    }
}
