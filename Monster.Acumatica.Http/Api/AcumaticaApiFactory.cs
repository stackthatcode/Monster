using System;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Push.Foundation.Web.Misc;

namespace Monster.Acumatica.Api
{
    public class AcumaticaApiFactory
    {
        private readonly AcumaticaHttpSettings _acumaticaHttpSettings;

        // Autofac factory to create Request Builder with Credentials wired-in
        private readonly 
            Func<AcumaticaSecuritySettings, AcumaticaRequestBuilder> _requestBuilderFactory;

        // Factory enables deliberate injection of ShopifyRequestBuilder and ShopifyClientSettings
        private readonly Func<Executor> _clientFacadeFactory;
        
        // Autofac factories for API Repositories
        private readonly Func<Executor, CustomerRepository> _spikeRepositoryFactory;
        

        public AcumaticaApiFactory(
                Func<AcumaticaSecuritySettings, AcumaticaRequestBuilder> requestBuilderFactory, 
                Func<Executor> clientFacadeFactory, 
                Func<Executor, CustomerRepository> spikeRepositoryFactory, 
                AcumaticaHttpSettings acumaticaHttpSettings)
        {
            _requestBuilderFactory = requestBuilderFactory;
            _clientFacadeFactory = clientFacadeFactory;
            _spikeRepositoryFactory = spikeRepositoryFactory;
            _acumaticaHttpSettings = acumaticaHttpSettings;
        }
        
        public virtual CustomerRepository MakeSpikeRepository(
                            AcumaticaSecuritySettings credentials)
        {
            var requestBuilder = _requestBuilderFactory(credentials);

            var clientFacade =
                _clientFacadeFactory()
                    .InjectRequestBuilder(requestBuilder)
                    .InjectSettings(_acumaticaHttpSettings);

            var repository = _spikeRepositoryFactory(clientFacade);
            return repository;
        }
    }
}

