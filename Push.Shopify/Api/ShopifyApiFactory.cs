using System;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Http;
using Push.Foundation.Web.Misc;
using Push.Shopify.Config;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;


namespace Push.Shopify.Api
{
    public class ShopifyApiFactory
    {
        // Solicit Autofac for our specific derivation of ClientSettings
        private readonly ShopifyClientSettings _clientSettings;
        
        // Autofac factories
        private readonly ShopifyHttpClientFactory _httpClientFactory;
        private readonly Func<IPushLogger> _loggerFactory;

        private readonly 
            Func<HttpFacade, OrderRepository> _orderRepositoryFactory;
        private readonly 
            Func<HttpFacade, ProductRepository> _productRepositoryFactory;
        private readonly 
            Func<HttpFacade, EventRepository> _eventRepositoryFactory;
        private readonly 
            Func<HttpFacade, ShopRepository> _shopRepositoryFactory;
        private readonly
            Func<HttpFacade, PayoutRepository> _payoutRepositoryFactory;

        private readonly Func<HttpFacade, InventoryRepository> _inventoryRepositoryFactory;


        public ShopifyApiFactory(
                ShopifyClientSettings clientSettings,
                ShopifyHttpClientFactory httpClientFactory,
                Func<IPushLogger> loggerFactory, 
                Func<HttpFacade, OrderRepository> orderRepositoryFactory,
                Func<HttpFacade, ProductRepository> productRepositoryFactory,
                Func<HttpFacade, EventRepository> eventRepositoryFactory,
                Func<HttpFacade, ShopRepository> shopRepositoryFactory,
                Func<HttpFacade, PayoutRepository> payoutRepositoryFactory,
                Func<HttpFacade, InventoryRepository> inventoryRepositoryFactory)
        {
            _clientSettings = clientSettings;
            _httpClientFactory = httpClientFactory;
            _loggerFactory = loggerFactory;

            _orderRepositoryFactory = orderRepositoryFactory;
            _productRepositoryFactory = productRepositoryFactory;
            _eventRepositoryFactory = eventRepositoryFactory;
            _shopRepositoryFactory = shopRepositoryFactory;
            _payoutRepositoryFactory = payoutRepositoryFactory;
            _inventoryRepositoryFactory = inventoryRepositoryFactory;
        }


        public HttpFacade MakeFacade(IShopifyCredentials credentials)
        {
            var client = _httpClientFactory.Make(credentials);

            var executionContext = new ExecutionContext()
            {
                NumberOfAttempts = _clientSettings.RetryLimit,
                ThrottlingKey = credentials.Domain.BaseUrl,
                Logger = _loggerFactory(),
            };

            return new HttpFacade(client, executionContext);
        }

        public virtual OrderRepository MakeOrderApi(IShopifyCredentials credentials)
        {
            var facade = MakeFacade(credentials);
            return _orderRepositoryFactory(facade);
        }
        
        public virtual ProductRepository MakeProductApi(IShopifyCredentials credentials)
        {
            var facade = MakeFacade(credentials);
            return _productRepositoryFactory(facade);
        }
        
        public virtual EventRepository MakeEventApi(IShopifyCredentials credentials)
        {
            var facade = MakeFacade(credentials);
            var repository = _eventRepositoryFactory(facade);
            return repository;
        }

        public virtual PayoutRepository MakePayoutApi(IShopifyCredentials credentials)
        {
            var facade = MakeFacade(credentials);
            var repository = _payoutRepositoryFactory(facade);
            return repository;
        }

        public virtual ShopRepository MakeShopApi(IShopifyCredentials credentials)
        {
            var facade = MakeFacade(credentials);
            var repository = _shopRepositoryFactory(facade);
            return repository;
        }

        public virtual InventoryRepository 
                MakeInventoryApi(IShopifyCredentials credentials)
        {
            var facade = MakeFacade(credentials);
            var repository = _inventoryRepositoryFactory(facade);
            return repository;
        }
    }
}

