using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Misc
{
    public static class LogBuilder
    {
        public static string UpdateShopifyPrice(AcumaticaStockItem item)
        {
            return $"Updating Shopify Price {item.LogDescriptor()}";
        }

        public static string UpdateShopifyInventory(AcumaticaStockItem item)
        {
            return $"Updating Shopify Inventory {item.LogDescriptor()}";
        }

        public static string CreateShopifyFulfillment(AcumaticaSoShipment shipmentRef)
        {
            return $"Creating Shopify Fulfillment for {shipmentRef.LogDescriptor()}";
        }

        public static string CreateStockItem(ShopifyVariant variant)
        {
            return $"Creating Acumatica Stock Item from {variant.LogDescriptor()}";
        }

        public static string CreateInventoryReceipt(long shopifyProductId)
        {
            return $"Creating Acumatica Inventory Receipt for Shopify Product: ({shopifyProductId})";
        }

        public static string CreateAcumaticaCustomer(ShopifyCustomer customer)
        {
            return $"Creating Acumatica Customer from {customer.LogDescriptor()}";
        }

        public static string UpdateAcumaticaCustomer(ShopifyCustomer customer)
        {
            return $"Updating Acumatica Customer from {customer.LogDescriptor()}";
        }

        public static string CreateAcumaticaSalesOrder(ShopifyOrder order)
        {
            return $"Creating Acumatica Sales Order from {order.LogDescriptor()}";
        }
        public static string UpdatingAcumaticaSalesOrder(ShopifyOrder order)
        {
            return $"Updating Acumatica Sales Order from {order.LogDescriptor()}";
        }

        public static string UpdateShopifyVariantPrice(string sku, decimal price)
        {
            return $"Updatingd Shopify Variant {sku} price to {price}";
        }

        public static string CreateAcumaticaPayment(ShopifyTransaction transaction)
        {
            return $"Creating Acumatica Payment from {transaction.LogDescriptor()}";
        }

        public static string UpdateAcumaticaPayment(ShopifyTransaction transaction)
        {
            return $"Updating Acumatica Payment from {transaction.LogDescriptor()}";
        }

        public static string CreateAcumaticaCustomerRefund(ShopifyTransaction transaction)
        {
            return $"Creating Acumatica Customer Refund from {transaction.LogDescriptor()}";
        }

        public static string ReleasingTransaction(ShopifyTransaction transaction)
        {
            return $"Releasing Acumatica record for {transaction.LogDescriptor()}";
        }
    }
}

