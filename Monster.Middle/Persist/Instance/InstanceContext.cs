using System;
using Monster.Acumatica.Http;
using Monster.Middle.Misc.External;
using Monster.Middle.Misc.State;
using Monster.Middle.Persist.Master;
using Push.Shopify.Http;


namespace Monster.Middle.Persist.Instance
{
    public class InstanceContext
    {
        private readonly InstanceRepository _systemRepository;
        private readonly ExternalServiceRepository _connectionRepository;
        private readonly ProcessPersistContext _processPersistContext;
        private readonly MiscPersistContext _miscPersistContext;
        private readonly ShopifyHttpContext _shopifyHttpContext;
        private readonly StateRepository _stateRepository;
        private readonly AcumaticaHttpContext _acumaticaHttpContext;
        
        // Why not add pass throughs to Shopify and Acumatica...?
        // Lifetime Scope says we're safe
        //
        private Guid? _instanceId;

        public readonly Guid ConnectionIdentifier = Guid.NewGuid();

        public InstanceContext(
                InstanceRepository systemRepository, 
                ProcessPersistContext processPersistContext,
                MiscPersistContext miscPersistContext,
                ExternalServiceRepository connectionRepository,
                ShopifyHttpContext shopifyHttpContext,
                StateRepository stateRepository,
                AcumaticaHttpContext acumaticaHttpContext)
        {
            _connectionRepository = connectionRepository;
            _systemRepository = systemRepository;
            _processPersistContext = processPersistContext;
            _miscPersistContext = miscPersistContext;
            _shopifyHttpContext = shopifyHttpContext;
            _stateRepository = stateRepository;
            _acumaticaHttpContext = acumaticaHttpContext;
        }

        
        public Guid InstanceId => _instanceId.Value;

        public void Initialize(Guid instanceId)
        {
            _instanceId = instanceId;
            var instance = _systemRepository.RetrieveInstance(instanceId);

            // Load the Instance Connection String
            // 
            _processPersistContext.Initialize(instance.ConnectionString);
            _miscPersistContext.Initialize(instance.ConnectionString);

            InitializeShopify(instanceId);
            InitializeAcumatica(instanceId);
        }

        public void InitializeShopify(Guid instanceId)
        {  
            var shopifyCredentials = _connectionRepository.RetrieveShopifyCredentials();
            if (shopifyCredentials != null)
            {
                _shopifyHttpContext.Initialize(shopifyCredentials);
            }
        }

        public void InitializeAcumatica(Guid instanceId)
        {
            var acumaticaCredentials = _connectionRepository.RetrieveAcumaticaCredentials();
            if (acumaticaCredentials != null)
            {
                _acumaticaHttpContext.Initialize(acumaticaCredentials);
            }
        }


        public void InitializePersistOnly(Guid instanceId)
        {
            _instanceId = instanceId;
            var instance = _systemRepository.RetrieveInstance(instanceId);
            _miscPersistContext.Initialize(instance.ConnectionString);
            _processPersistContext.Initialize(instance.ConnectionString);
        }

        public void UpdateShopifyCredentials(string shop, string accessToken, string codeHash)
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
