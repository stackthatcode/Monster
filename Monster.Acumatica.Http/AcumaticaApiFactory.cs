using System;
using Push.Foundation.Web.HttpClient;

// ReSharper disable once RedundantUsingDirective

namespace Monster.Acumatica.Http
{
    public class AcumaticaApiFactory
    {
        private readonly AcumaticaHttpSettings _acumaticaHttpSettings;

        // Autofac factory to create Request Builder with Credentials wired-in
        private readonly 
            Func<AcumaticaCredentials, AcumaticaRequestBuilder> _requestBuilderFactory;

        // Factory enables deliberate injection of ShopifyRequestBuilder and ShopifyClientSettings
        private readonly 
            Func<AcumaticaRequestBuilder, AcumaticaHttpSettings, HttpFacade> _clientFacadeFactory;
        
        // Autofac factories for API Repositories
        private readonly Func<HttpFacade, SpikeRepository> _spikeRepositoryFactory;
        

        public AcumaticaApiFactory(
            Func<AcumaticaCredentials, AcumaticaRequestBuilder> requestBuilderFactory, 
            Func<AcumaticaRequestBuilder, AcumaticaHttpSettings, HttpFacade> clientFacadeFactory, 
            Func<HttpFacade, SpikeRepository> spikeRepositoryFactory, 
            AcumaticaHttpSettings acumaticaHttpSettings)
        {
            _requestBuilderFactory = requestBuilderFactory;
            _clientFacadeFactory = clientFacadeFactory;
            _spikeRepositoryFactory = spikeRepositoryFactory;
            _acumaticaHttpSettings = acumaticaHttpSettings;
        }
        
        public virtual SpikeRepository MakeSpikeRepository(AcumaticaCredentials credentials)
        {
            var requestBuilder = _requestBuilderFactory(credentials);
            var clientFacade = _clientFacadeFactory(requestBuilder, _acumaticaHttpSettings);
            var repository = _spikeRepositoryFactory(clientFacade);
            return repository;
        }
    }
}

