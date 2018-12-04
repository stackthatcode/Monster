using Monster.Middle.Processes.Shopify.Workers;


namespace Monster.Middle.Processes.Shopify
{
    public class ShopifyManager
    {
        private readonly ShopifyLocationPull _shopifyLocationPull;
        private readonly ShopifyInventoryPull _shopifyInventoryPull;
        private readonly ShopifyCustomerPull _shopifyCustomerPull;
        private readonly ShopifyOrderPull _shopifyOrderPull;
        private readonly ShopifyTransactionPull _shopifyTransactionPull;

        public ShopifyManager(
            ShopifyLocationPull shopifyLocationPull,
            ShopifyInventoryPull shopifyInventoryPull, 
            ShopifyCustomerPull shopifyCustomerPull, 
            ShopifyOrderPull shopifyOrderPull, 
            ShopifyTransactionPull shopifyTransactionPull)
        {
            _shopifyLocationPull = shopifyLocationPull;
            _shopifyInventoryPull = shopifyInventoryPull;
            _shopifyCustomerPull = shopifyCustomerPull;
            _shopifyOrderPull = shopifyOrderPull;
            _shopifyTransactionPull = shopifyTransactionPull;
        }
        


        public void PullLocations()
        {
            _shopifyLocationPull.Run();
        }

        public void PullInventory()
        {
            _shopifyInventoryPull.RunAutomatic();
        }

        public void PullOrdersAndCustomers()
        {
            _shopifyCustomerPull.RunAutomatic();
            _shopifyOrderPull.RunAutomatic();
            _shopifyTransactionPull.RunAutomatic();
        }
    }
}
