using System;
using System.Net.Http;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Http;
using Push.Foundation.Web.Misc;

namespace Monster.Acumatica.Api
{
    public class AcumaticaApiFactory
    {
        private readonly AcumaticaHttpSettings _acumaticaHttpSettings;
        private readonly AcumaticaHttpClientFactory _httpClientFactory;

        // Autofac factories
        private readonly Func<IPushLogger> _loggerFactory;

        private readonly Func<HttpClient, ExecutionContext, HttpFacade> _facadeFactory;

        private readonly Func<HttpFacade, SessionRepository> _sessionApiFactory;
        private readonly Func<HttpFacade, UrlBuilder, CustomerRepository> _customerApiFactory;
        private readonly Func<HttpFacade, UrlBuilder, BankRepository> _bankApiFactory;
        private readonly Func<HttpFacade, UrlBuilder, InventoryRepository> _inventoryApiFactory;


        public AcumaticaApiFactory(
                AcumaticaHttpSettings acumaticaHttpSettings,
                AcumaticaHttpClientFactory httpClientFactory, 
                
                Func<IPushLogger> loggerFactory,
                
                Func<HttpClient, ExecutionContext, HttpFacade> facadeFactory,
                
                Func<HttpFacade, SessionRepository> sessionApiFactory,
                Func<HttpFacade, UrlBuilder, CustomerRepository> customerApiFactory,
                Func<HttpFacade, UrlBuilder, BankRepository> bankApiFactory,
                Func<HttpFacade, UrlBuilder, InventoryRepository> inventoryApiFactory)
        {
            _acumaticaHttpSettings = acumaticaHttpSettings;
            _httpClientFactory = httpClientFactory;

            _loggerFactory = loggerFactory;

            _facadeFactory = facadeFactory;

            _sessionApiFactory = sessionApiFactory;
            _customerApiFactory = customerApiFactory;
            _bankApiFactory = bankApiFactory;
            _inventoryApiFactory = inventoryApiFactory;
        }

        public HttpFacade MakeFacade(AcumaticaCredentials credentials)
        {
            var client = _httpClientFactory.Make(credentials);

            var executionContext = new ExecutionContext()
            {
                NumberOfAttempts = _acumaticaHttpSettings.RetryLimit,
                ThrottlingKey = credentials.InstanceUrl,
                Logger = _loggerFactory(),
            };

            var output = _facadeFactory(client, executionContext);
            return output;
        }

        public UrlBuilder MakeUrlBuilder(
                    AcumaticaCredentials credentials, 
                    AcumaticaHttpSettings settings)
        {
            return new UrlBuilder(
                    credentials.InstanceUrl, settings.VersionSegment);
        }
        
        public virtual SessionRepository
                MakeSessionRepository(AcumaticaCredentials credentials)
        {
            var facade = MakeFacade(credentials);
            var repository = _sessionApiFactory(facade);
            return repository;
        }

        public virtual CustomerRepository 
                MakeCustomerRepository(AcumaticaCredentials credentials)
        {
            var facade = MakeFacade(credentials);
            var urlBuilder = MakeUrlBuilder(credentials, _acumaticaHttpSettings);
            var repository = _customerApiFactory(facade, urlBuilder);
            return repository;
        }

        public virtual BankRepository
                MakeBankRepository(AcumaticaCredentials credentials)
        {
            var facade = MakeFacade(credentials);
            var urlBuilder = MakeUrlBuilder(credentials, _acumaticaHttpSettings);
            var repository = _bankApiFactory(facade, urlBuilder);
            return repository;
        }

        public virtual InventoryRepository
                MakeInventoryRepository(AcumaticaCredentials credentials)
        {
            var facade = MakeFacade(credentials);
            var urlBuilder = MakeUrlBuilder(credentials, _acumaticaHttpSettings);
            var repository = _inventoryApiFactory(facade, urlBuilder);
            return repository;
        }
    }
}

