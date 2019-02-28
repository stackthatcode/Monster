using System;
using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Sys;
using Push.Shopify.Http;


namespace Monster.Middle.Security
{
    public class ConnectionContext
    {
        private readonly SystemRepository _systemRepository;
        private readonly ConnectionRepository _connectionRepository;
        private readonly PersistContext _persistContext;
        private readonly ShopifyHttpContext _shopifyHttpContext;
        private readonly AcumaticaHttpContext _acumaticaHttpContext;
        
        // Why not add pass throughs to Shopify and Acumatica...?
        // Lifetime Scope says we're safe
        
        private Guid? _instanceId;

        public readonly Guid ConnectionIdentifier = Guid.NewGuid();

        public ConnectionContext(
                ConnectionRepository connectionRepository, 
                SystemRepository systemRepository, 
                PersistContext persistContext, 
                ShopifyHttpContext shopifyHttpContext,
                AcumaticaHttpContext acumaticaHttpContext)
        {
            _connectionRepository = connectionRepository;
            _systemRepository = systemRepository;
            _persistContext = persistContext;
            _shopifyHttpContext = shopifyHttpContext;
            _acumaticaHttpContext = acumaticaHttpContext;
        }

        
        public Guid InstanceId => _instanceId.Value;

        public void Initialize(Guid instanceId)
        {
            _instanceId = instanceId;
            var instance = _systemRepository.RetrieveInstance(instanceId);

            // Load the Installation into Persist 
            _persistContext.Initialize(instance.ConnectionString);

            // Shopify
            var shopifyCredentials = _connectionRepository.RetrieveShopifyCredentials();
            _shopifyHttpContext.Initialize(shopifyCredentials);

            // Acumatica
            var acumaticaCredentials = _connectionRepository.RetrieveAcumaticaCredentials();
            _acumaticaHttpContext.Initialize(acumaticaCredentials);
        }

        public void InitializePersistOnly(Guid instanceId)
        {
            _instanceId = instanceId;
            var installation = _systemRepository.RetrieveInstance(instanceId);
            _persistContext.Initialize(installation.ConnectionString);
        }        
        
    }
}
