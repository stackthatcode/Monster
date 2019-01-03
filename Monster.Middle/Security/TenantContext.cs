using System;
using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Sys.Repositories;
using Push.Shopify.Http;


namespace Monster.Middle.Security
{
    public class TenantContext
    {
        private readonly SystemRepository _systemRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly PersistContext _persistContext;
        private readonly ShopifyHttpContext _shopifyHttpContext;
        private readonly AcumaticaHttpContext _acumaticaHttpContext;
        
        // Why not add pass throughs Shopify and Acumatica...?
        // Lifetime Scope says we're safe
        private Guid? _tenantId;


        public TenantContext(
                TenantRepository tenantRepository, 
                SystemRepository systemRepository, 
                PersistContext persistContext, 
                ShopifyHttpContext shopifyHttpContext,
                AcumaticaHttpContext acumaticaHttpContext)
        {
            _tenantRepository = tenantRepository;
            _systemRepository = systemRepository;
            _persistContext = persistContext;
            _shopifyHttpContext = shopifyHttpContext;
            _acumaticaHttpContext = acumaticaHttpContext;
        }

        public Guid InstallationId => _tenantId.Value;

        public void Initialize(Guid installationId)
        {
            _tenantId = installationId;

            var installation = 
                    _systemRepository
                        .RetrieveInstallation(installationId);

            // Load the Installation into Persist 
            _persistContext.Initialize(
                    installation.ConnectionString, installation.CompanyId);

            // Shopify
            var shopifyCredentials
                    = _tenantRepository.RetrieveShopifyCredentials();
            _shopifyHttpContext.Initialize(shopifyCredentials);

            // Acumatica
            var acumaticaCredentials
                    = _tenantRepository.RetrieveAcumaticaCredentials();
            _acumaticaHttpContext.Initialize(acumaticaCredentials);
        }

        public void InitializePersistOnly(Guid installationId)
        {
            _tenantId = installationId;

            var installation = 
                    _systemRepository.RetrieveInstallation(installationId);

            // Load the Installation into Persist 
            _persistContext.Initialize(
                installation.ConnectionString, installation.CompanyId);

        }        
    }
}
