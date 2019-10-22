using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Misc
{
    public static class LogBuilder
    {
        public static string UpdateShopifyPrice(AcumaticaStockItem item)
        {
            return $"Update Shopify Price {item.LogDescriptor()}";
        }

        public static string UpdateShopifyInventory(AcumaticaStockItem item)
        {
            return $"Update Shopify Inventory {item.LogDescriptor()}";
        }

        public static string 
                    CreateShopifyFulfillment(AcumaticaShipmentSalesOrderRef shipmentRef)
        {
            return $"Create Shopify Fulfillment {shipmentRef.LogDescriptor()}";
        }

        public static string CreateStockItem(ShopifyVariant variant)
        {
            return $"Create Acumatica Stock Item from {variant.LogDescriptor()}";
        }

        public static string CreateInventoryReceipt(long shopifyProductId)
        {
            return $"Create Acumatica Inventory Receipt for Shopify Product: ({shopifyProductId})";
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
            return $"Create Acumatica Sales Order from {order.LogDescriptor()}";
        }

        public static string UpdateShopifyVariantPrice(string sku, decimal price)
        {
            return $"Updated Shopify Variant {sku} price to {price}";
        }

        public static string CreateAcumaticaPayment(ShopifyTransaction transaction)
        {
            return $"Create Acumatica Payment from {transaction.LogDescriptor()}";
        }

        public static string CreateAcumaticaCreditMemo(ShopifyRefund refund)
        {
            return $"Create Acumatica Credit Memo Order from {refund.LogDescriptor()}";
        }
    }
}

