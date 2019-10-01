using System;
using Monster.Acumatica.Http;
using Monster.Middle.Persist.Master;
using Monster.Middle.Processes.Misc;
using Monster.Middle.Processes.Sync.Model.Misc;
using Push.Shopify.Http;


namespace Monster.Middle.Persist.Instance
{
    public class InstanceContext
    {
        private readonly SystemRepository _systemRepository;
        private readonly ExternalServiceRepository _connectionRepository;
        private readonly InstancePersistContext _persistContext;
        private readonly ShopifyHttpContext _shopifyHttpContext;
        private readonly StateRepository _stateRepository;
        private readonly AcumaticaHttpContext _acumaticaHttpContext;
        
        // Why not add pass throughs to Shopify and Acumatica...?
        // Lifetime Scope says we're safe
        
        private Guid? _instanceId;

        public readonly Guid ConnectionIdentifier = Guid.NewGuid();

        public InstanceContext(
                ExternalServiceRepository connectionRepository, 
                SystemRepository systemRepository, 
                InstancePersistContext persistContext, 
                ShopifyHttpContext shopifyHttpContext,
                StateRepository stateRepository,
                AcumaticaHttpContext acumaticaHttpContext)
        {
            _connectionRepository = connectionRepository;
            _systemRepository = systemRepository;
            _persistContext = persistContext;
            _shopifyHttpContext = shopifyHttpContext;
            _stateRepository = stateRepository;
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
            if (shopifyCredentials != null)
            {
                _shopifyHttpContext.Initialize(shopifyCredentials);
            }

            // Acumatica
            var acumaticaCredentials = _connectionRepository.RetrieveAcumaticaCredentials();
            if (acumaticaCredentials != null)
            {
                _acumaticaHttpContext.Initialize(acumaticaCredentials);
            }
        }

        public void InitializePersistOnly(Guid instanceId)
        {
            _instanceId = instanceId;
            var installation = _systemRepository.RetrieveInstance(instanceId);
            _persistContext.Initialize(installation.ConnectionString);
        }

        public void UpdateShopifyConnectionAndCodeHash(string shop, string accessToken, string codeHash)
        {
            using (var transaction = _connectionRepository.BeginTransaction())
            {
                _connectionRepository.UpdateShopifyCredentials(shop, accessToken, codeHash);
                _stateRepository.UpdateSystemState(x => x.ShopifyConnState, StateCode.Ok);
                _stateRepository.UpdateSystemState(x => x.IsShopifyUrlFinalized, true);
                transaction.Commit();
            }
        }
    }
}
