using Push.Shopify.Api;

namespace Monster.Middle.Processes.Shopify.Workers
{
    public class ShopifyManager
    {
        private readonly ShopifyLocationGet _shopifyLocationPull;
        private readonly ShopifyInventoryGet _shopifyInventoryPull;
        private readonly ShopifyCustomerGet _shopifyCustomerPull;
        private readonly ShopifyOrderGet _shopifyOrderPull;
        private readonly ShopifyTransactionGet _shopifyTransactionPull;
        private readonly OrderApi _orderApi;

        public ShopifyManager(
            ShopifyLocationGet shopifyLocationPull,
            ShopifyInventoryGet shopifyInventoryPull, 
            ShopifyCustomerGet shopifyCustomerPull, 
            ShopifyOrderGet shopifyOrderPull, 
            ShopifyTransactionGet shopifyTransactionPull, 
            OrderApi orderApi)
        {
            _shopifyLocationPull = shopifyLocationPull;
            _shopifyInventoryPull = shopifyInventoryPull;
            _shopifyCustomerPull = shopifyCustomerPull;
            _shopifyOrderPull = shopifyOrderPull;
            _shopifyTransactionPull = shopifyTransactionPull;
            _orderApi = orderApi;
        }

        
        public void TestConnection()
        {
            _orderApi.RetrieveCount();
        }
        
        public void PullLocations()
        {
            _shopifyLocationPull.Run();
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
