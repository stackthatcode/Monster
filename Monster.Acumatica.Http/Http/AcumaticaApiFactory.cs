using System;
using Monster.Acumatica.Config;
using Push.Foundation.Web.HttpClient;

namespace Monster.Acumatica.Http
{
    public class AcumaticaApiFactory
    {
        private readonly AcumaticaHttpSettings _acumaticaHttpSettings;

        // Autofac factory to create Request Builder with Credentials wired-in
        private readonly 
            Func<AcumaticaSecuritySettings, AcumaticaRequestBuilder> _requestBuilderFactory;

        // Factory enables deliberate injection of ShopifyRequestBuilder and ShopifyClientSettings
        private readonly Func<HttpFacade> _clientFacadeFactory;
        
        // Autofac factories for API Repositories
        private readonly Func<HttpFacade, Repository> _spikeRepositoryFactory;
        

        public AcumaticaApiFactory(
                Func<AcumaticaSecuritySettings, AcumaticaRequestBuilder> requestBuilderFactory, 
                Func<HttpFacade> clientFacadeFactory, 
                Func<HttpFacade, Repository> spikeRepositoryFactory, 
                AcumaticaHttpSettings acumaticaHttpSettings)
        {
            _requestBuilderFactory = requestBuilderFactory;
            _clientFacadeFactory = clientFacadeFactory;
            _spikeRepositoryFactory = spikeRepositoryFactory;
            _acumaticaHttpSettings = acumaticaHttpSettings;
        }
        
        public virtual Repository MakeSpikeRepository(
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

