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
        // Autofac factory for Request Builders
        private readonly Func<IShopifyCredentials, ShopifyRequestBuilder> _requestBuilderFactory;

        // Solicit Autofac for our specific derivation of ClientSettings
        private readonly ShopifyClientSettings _clientSettings;

        // Factory enables deliberate injection of ShopifyClientSettings
        private readonly Func<HttpSettings, HttpFacade> _clientFacadeFactory;
        
        // Autofac factories for API Repositories
        private readonly 
            Func<ShopifyRequestBuilder, HttpFacade, OrderApiRepository> _orderApiRepositoryFactory;
        private readonly 
            Func<ShopifyRequestBuilder, HttpFacade, ProductApiRepository> _productApiRepositoryFactory;
        private readonly 
            Func<ShopifyRequestBuilder, HttpFacade, EventApiRepository> _eventApiRepositoryFactory;
        private readonly 
            Func<ShopifyRequestBuilder, HttpFacade, ShopApiRepository> _shopApiRepositoryFactory;
        private readonly
            Func<ShopifyRequestBuilder, HttpFacade, PayoutApiRepository> _payoutApiRepositoryFactory;



        public ApiFactory(
                Func<IShopifyCredentials, ShopifyRequestBuilder> requestBuilderFactory,
                Func<HttpSettings, HttpFacade> clientFacadeFactory,
                
                ShopifyClientSettings clientSettings,

                Func<ShopifyRequestBuilder, HttpFacade, OrderApiRepository> orderApiRepositoryFactory,
                Func<ShopifyRequestBuilder, HttpFacade, ProductApiRepository> productApiRepositoryFactory,
                Func<ShopifyRequestBuilder, HttpFacade, EventApiRepository> eventApiRepositoryFactory,
                Func<ShopifyRequestBuilder, HttpFacade, ShopApiRepository> shopApiRepositoryFactory,
                Func<ShopifyRequestBuilder, HttpFacade, PayoutApiRepository> payoutApiRepositoryFactory)
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
            var clientFacade = _clientFacadeFactory(_clientSettings);
            var repository = _orderApiRepositoryFactory(requestBuilder, clientFacade);
            return repository;
        }
        
        public virtual ProductApiRepository MakeProductApi(IShopifyCredentials credentials)
        {
            var requestBuilder = _requestBuilderFactory(credentials);
            var clientFacade = _clientFacadeFactory(_clientSettings);
            var repository = _productApiRepositoryFactory(requestBuilder, clientFacade);
            return repository;
        }
        
        public virtual EventApiRepository MakeEventApi(IShopifyCredentials credentials)
        {
            var requestBuilder = _requestBuilderFactory(credentials);
            var clientFacade = _clientFacadeFactory(_clientSettings);
            var repository = _eventApiRepositoryFactory(requestBuilder, clientFacade);
            return repository;
        }

        public virtual PayoutApiRepository MakePayoutApi(IShopifyCredentials credentials)
        {
            var requestBuilder = _requestBuilderFactory(credentials);
            var clientFacade = _clientFacadeFactory(_clientSettings);
            var repository = _payoutApiRepositoryFactory(requestBuilder, clientFacade);
            return repository;
        }

        public virtual ShopApiRepository MakeShopApi(IShopifyCredentials credentials)
        {
            var requestBuilder = _requestBuilderFactory(credentials);
            var clientFacade = _clientFacadeFactory(_clientSettings);
            var repository = _shopApiRepositoryFactory(requestBuilder, clientFacade);
            return repository;
        }
    }
}

