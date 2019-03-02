using Monster.Middle.Processes.Shopify.Workers;
using Push.Shopify.Api;


namespace Monster.Middle.Processes.Shopify
{
    public class ShopifyManager
    {
        private readonly ShopifyLocationPull _shopifyLocationPull;
        private readonly ShopifyInventoryPull _shopifyInventoryPull;
        private readonly ShopifyCustomerPull _shopifyCustomerPull;
        private readonly ShopifyOrderPull _shopifyOrderPull;
        private readonly ShopifyTransactionPull _shopifyTransactionPull;
        private readonly OrderApi _orderApi;

        public ShopifyManager(
            ShopifyLocationPull shopifyLocationPull,
            ShopifyInventoryPull shopifyInventoryPull, 
            ShopifyCustomerPull shopifyCustomerPull, 
            ShopifyOrderPull shopifyOrderPull, 
            ShopifyTransactionPull shopifyTransactionPull, 
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
