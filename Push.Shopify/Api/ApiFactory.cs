using System;
using Push.Foundation.Web.HttpClient;
using Push.Shopify.Config;
using Push.Shopify.Credentials;
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
        private readonly Func<ClientSettings, ClientFacade> _clientFacadeFactory;
        
        // Autofac factories for API Repositories
        private readonly 
            Func<ShopifyRequestBuilder, ClientFacade, OrderApiRepository> _orderApiRepositoryFactory;
        private readonly 
            Func<ShopifyRequestBuilder, ClientFacade, ProductApiRepository> _productApiRepositoryFactory;
        private readonly 
            Func<ShopifyRequestBuilder, ClientFacade, EventApiRepository> _eventApiRepositoryFactory;
        private readonly 
            Func<ShopifyRequestBuilder, ClientFacade, ShopApiRepository> _shopApiRepositoryFactory;
        


        public ApiFactory(
                Func<IShopifyCredentials, ShopifyRequestBuilder> requestBuilderFactory,
                Func<ClientSettings, ClientFacade> clientFacadeFactory,
                
                Func<ShopifyRequestBuilder, ClientFacade, OrderApiRepository> orderApiRepositoryFactory,
                Func<ShopifyRequestBuilder, ClientFacade, ProductApiRepository> productApiRepositoryFactory,
                Func<ShopifyRequestBuilder, ClientFacade, EventApiRepository> eventApiRepositoryFactory,
                Func<ShopifyRequestBuilder, ClientFacade, ShopApiRepository> shopApiRepositoryFactory,
                
                ShopifyClientSettings clientSettings)
        {
            _requestBuilderFactory = requestBuilderFactory;

            _clientSettings = clientSettings;
            _clientFacadeFactory = clientFacadeFactory;

            _orderApiRepositoryFactory = orderApiRepositoryFactory;
            _productApiRepositoryFactory = productApiRepositoryFactory;
            _eventApiRepositoryFactory = eventApiRepositoryFactory;
            _shopApiRepositoryFactory = shopApiRepositoryFactory;
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


        public virtual ShopApiRepository MakeShopApi(IShopifyCredentials credentials)
        {
            var requestBuilder = _requestBuilderFactory(credentials);
            var clientFacade = _clientFacadeFactory(_clientSettings);
            var repository = _shopApiRepositoryFactory(requestBuilder, clientFacade);
            return repository;
        }
    }
}

