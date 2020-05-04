using System;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.State;
using Monster.Middle.Processes.Shopify.Workers;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;

namespace Monster.Middle.Processes.Shopify
{
    public class ShopifyManager
    {
        private readonly ShopifyReferenceGet _shopifyReferenceGet;
        private readonly ShopifyInventoryGet _shopifyInventoryPull;
        private readonly ShopifyCustomerGet _shopifyCustomerPull;
        private readonly ShopifyOrderGet _shopifyOrderPull;
        private readonly ShopifyTransactionGet _shopifyTransactionPull;
        private readonly ExecutionLogService _executionLogService;
        private readonly StateRepository _stateRepository;
        private readonly IPushLogger _logger;
        private readonly OrderApi _orderApi;

        public ShopifyManager(
            ShopifyReferenceGet shopifyReferenceGet,
            ShopifyInventoryGet shopifyInventoryPull, 
            ShopifyCustomerGet shopifyCustomerPull, 
            ShopifyOrderGet shopifyOrderPull, 
            ShopifyTransactionGet shopifyTransactionPull, 
            ExecutionLogService executionLogService,
            StateRepository stateRepository,
            IPushLogger logger,
            OrderApi orderApi)
        {
            _shopifyReferenceGet = shopifyReferenceGet;
            _shopifyInventoryPull = shopifyInventoryPull;
            _shopifyCustomerPull = shopifyCustomerPull;
            _shopifyOrderPull = shopifyOrderPull;
            _shopifyTransactionPull = shopifyTransactionPull;
            _executionLogService = executionLogService;
            _stateRepository = stateRepository;
            _logger = logger;
            _orderApi = orderApi;
        }

        
        public void TestConnection()
        {
            try
            {
                _executionLogService.Log("Connecting to Shopify...");
                _orderApi.RetrieveCount();
                _stateRepository.UpdateSystemState(x => x.ShopifyConnState, StateCode.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository.UpdateSystemState(x => x.ShopifyConnState, StateCode.SystemFault);
            }
        }

        public void PullReferenceData()
        {
            _shopifyReferenceGet.RunShippingCarriers();
        }

        public void PullLocations()
        {
            _shopifyReferenceGet.RunLocations();
        }

        public void PullInventory()
        {
            _shopifyInventoryPull.RunAutomatic();
        }

        public void PullOrders()
        {
            _shopifyOrderPull.RunAutomatic();
        }

        public void PullCustomers()
        {
            _shopifyCustomerPull.RunAutomatic();
        }

        public void PullTransactions()
        {
            _shopifyTransactionPull.RunAutomatic();
        }
    }
}
