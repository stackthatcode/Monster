using Monster.Middle.Persist.Instance;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Sync.Misc
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

        public static string CreatingShopifyVariant(ShopifyVariantNew item)
        {
            return $"Creating Shopify Variant for {item.LogDescriptor()}";
        }

        public static string CreatedShopifyProduct(ShopifyProduct product)
        {
            return $"Created {product.LogDescriptor()}";
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

        public static string UpdateShopifyVariantPrice(
                string sku, bool taxable, decimal? price, int? grams)
        {
            var output = $"Updated Shopify Variant {sku} - taxable: {taxable}";
            if (price.HasValue)
            {
                output += $", price: {price}";
            }
            if (grams.HasValue)
            {
                output += $", weight (grams): {grams}";
            }
            return output;
        }

        public static string UpdateShopifyVariantCogsOfGoods(string sku, decimal costOfGoods)
        {
            return $"Updated Shopify Variant {sku} - cost of goods: {costOfGoods}";
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

        public static string JobExecutionIsInterrupted()
        {
            return $"Execution has been interrupted";
        }

        public static string SkippingInvalidShopifyOrder(long shopifyOrderId)
        {
            return $"Shopify Order {shopifyOrderId} block from writing to Acumatica";
        }
    }
}

