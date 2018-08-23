using System;
using Push.Foundation.Web.HttpClient;
using Push.Shopify.Config;
// ReSharper disable once RedundantUsingDirective
using Push.Shopify.HttpClient;
using Push.Shopify.HttpClient.Credentials;

namespace Push.Shopify.Api
{
    public class ApiFactory
    {
        // Solicit Autofac for our specific derivation of ClientSettings
        private readonly ShopifyClientSettings _clientSettings;

        // Autofac factory to create Request Builder with Credentials wired-in
        private readonly Func<IShopifyCredentials, ShopifyRequestBuilder> _requestBuilderFactory;

        // Factory enables deliberate injection of ShopifyRequestBuilder and ShopifyClientSettings
        private readonly Func<ShopifyRequestBuilder, HttpSettings, HttpFacade> _clientFacadeFactory;
        
        // Autofac factories for API Repositories
        private readonly 
            Func<HttpFacade, OrderApiRepository> _orderApiRepositoryFactory;
        private readonly 
            Func<HttpFacade, ProductApiRepository> _productApiRepositoryFactory;
        private readonly 
            Func<HttpFacade, EventApiRepository> _eventApiRepositoryFactory;
        private readonly 
            Func<HttpFacade, ShopApiRepository> _shopApiRepositoryFactory;
        private readonly
            Func<HttpFacade, PayoutApiRepository> _payoutApiRepositoryFactory;



        public ApiFactory(
                Func<IShopifyCredentials, ShopifyRequestBuilder> requestBuilderFactory,
                Func<ShopifyRequestBuilder, HttpSettings, HttpFacade> clientFacadeFactory,
                
                ShopifyClientSettings clientSettings,

                Func<HttpFacade, OrderApiRepository> orderApiRepositoryFactory,
                Func<HttpFacade, ProductApiRepository> productApiRepositoryFactory,
                Func<HttpFacade, EventApiRepository> eventApiRepositoryFactory,
                Func<HttpFacade, ShopApiRepository> shopApiRepositoryFactory,
                Func<HttpFacade, PayoutApiRepository> payoutApiRepositoryFactory)
        {
            _requestBuilderFactory = requestBuilderFactory;

            _clientSettings = clientSettings;
            _clientFacadeFactory = clientFacadeFactory;

            _orderApiRepositoryFactory = orderApiRepositoryFactory;
            _productApiRepositoryFactory = productApiRepositoryFactory;
            _eventApiRepositoryFactory = eventApiRepositoryFactory;
            _shopApiRepositoryFactory = shopApiRepositoryFactory;
            _payoutApiRepositoryFactory = payoutApiRepositoryFactory;
        }


        public virtual OrderApiRepository MakeOrderApi(IShopifyCredentials credentials)
        {
            var requestBuilder = _requestBuilderFactory(credentials);
            var clientFacade = _clientFacadeFactory(requestBuilder, _clientSettings);
            var repository = _orderApiRepositoryFactory(clientFacade);
            return repository;
        }
        
        public virtual ProductApiRepository MakeProductApi(IShopifyCredentials credentials)
        {
            var requestBuilder = _requestBuilderFactory(credentials);
            var clientFacade = _clientFacadeFactory(requestBuilder, _clientSettings);
            var repository = _productApiRepositoryFactory(clientFacade);
            return repository;
        }
        
        public virtual EventApiRepository MakeEventApi(IShopifyCredentials credentials)
        {
            var requestBuilder = _requestBuilderFactory(credentials);
            var clientFacade = _clientFacadeFactory(requestBuilder, _clientSettings);
            var repository = _eventApiRepositoryFactory(clientFacade);
            return repository;
        }

        public virtual PayoutApiRepository MakePayoutApi(IShopifyCredentials credentials)
        {
            var requestBuilder = _requestBuilderFactory(credentials);
            var clientFacade = _clientFacadeFactory(requestBuilder, _clientSettings);
            var repository = _payoutApiRepositoryFactory(clientFacade);
            return repository;
        }

        public virtual ShopApiRepository MakeShopApi(IShopifyCredentials credentials)
        {
            var requestBuilder = _requestBuilderFactory(credentials);
            var clientFacade = _clientFacadeFactory(requestBuilder, _clientSettings);
            var repository = _shopApiRepositoryFactory(clientFacade);
            return repository;
        }
    }
}

