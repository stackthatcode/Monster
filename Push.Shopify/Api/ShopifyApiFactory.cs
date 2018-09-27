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
        private readonly ShopifyHttpSettings _httpSettings;
        
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
                ShopifyHttpSettings httpSettings,

                ShopifyHttpClientFactory httpClientFactory,

                Func<IPushLogger> loggerFactory, 

                Func<HttpFacade, OrderRepository> orderRepositoryFactory,
                Func<HttpFacade, ProductRepository> productRepositoryFactory,
                Func<HttpFacade, EventRepository> eventRepositoryFactory,
                Func<HttpFacade, ShopRepository> shopRepositoryFactory,
                Func<HttpFacade, PayoutRepository> payoutRepositoryFactory,
                Func<HttpFacade, InventoryRepository> inventoryRepositoryFactory)
        {
            _httpSettings = httpSettings;
            _httpClientFactory = httpClientFactory;
            _loggerFactory = loggerFactory;

            _orderRepositoryFactory = orderRepositoryFactory;
            _productRepositoryFactory = productRepositoryFactory;
            _eventRepositoryFactory = eventRepositoryFactory;
            _shopRepositoryFactory = shopRepositoryFactory;
            _payoutRepositoryFactory = payoutRepositoryFactory;
            _inventoryRepositoryFactory = inventoryRepositoryFactory;
        }

        public IShopifyCredentials Credentials { get; private set; }

        public void SetCredentials(IShopifyCredentials credentials)
        {
            Credentials = credentials;
        }

        public HttpFacade MakeFacade()
        {
            var httpClient = _httpClientFactory.Make(Credentials);

            var executionContext = new DurableExecContext()
            {
                NumberOfAttempts = _httpSettings.RetryLimit,
                ThrottlingKey = Credentials.Domain.BaseUrl,
                Logger = _loggerFactory(),
            };

            return new HttpFacade(httpClient, executionContext);
        }

        public virtual OrderRepository MakeOrderApi()
        {
            var facade = MakeFacade();
            return _orderRepositoryFactory(facade);
        }
        
        public virtual ProductRepository MakeProductApi()
        {
            var facade = MakeFacade();
            return _productRepositoryFactory(facade);
        }
        
        public virtual EventRepository MakeEventApi()
        {
            var facade = MakeFacade();
            return _eventRepositoryFactory(facade);
        }

        public virtual PayoutRepository MakePayoutApi()
        {
            var facade = MakeFacade();
            return _payoutRepositoryFactory(facade);
        }

        public virtual ShopRepository MakeShopApi()
        {
            var facade = MakeFacade();
            return _shopRepositoryFactory(facade);
        }

        public virtual InventoryRepository MakeInventoryApi()
        {
            var facade = MakeFacade();
            return _inventoryRepositoryFactory(facade);
        }
    }
}

