using System;
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
        private readonly Func<HttpFacade, CustomerRepository> _repositoryFactory;


        public AcumaticaApiFactory(
                AcumaticaHttpSettings acumaticaHttpSettings,
                AcumaticaHttpClientFactory httpClientFactory, 
                Func<IPushLogger> loggerFactory,
                Func<HttpFacade, CustomerRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            _httpClientFactory = httpClientFactory;
            _loggerFactory = loggerFactory;
            _acumaticaHttpSettings = acumaticaHttpSettings;
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

            return new HttpFacade(client, executionContext);
        }

        public virtual CustomerRepository 
                    MakeSpikeRepository(
                        AcumaticaCredentials credentials)
        {
            var facade = MakeFacade(credentials);
            var repository = _repositoryFactory(facade);
            return repository;
        }
    }
}

