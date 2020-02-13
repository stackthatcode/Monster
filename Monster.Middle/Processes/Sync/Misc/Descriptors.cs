using Monster.Acumatica.Api.Customer;
using Monster.Middle.Persist.Instance;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Sync.Misc
{
    public static class Descriptors
    {
        public static string LogDescriptor(this ShopifyProduct shopifyProduct)
        {
            return $"Shopify Product {shopifyProduct.ShopifyProductId}";
        }

        public static string LogDescriptor(this ShopifyVariant variant)
        {
            return $"Shopify Variant {variant.ShopifySku} ({variant.ShopifyVariantId})";
        }

        public static string LogDescriptor(this ShopifyVariantNew variant)
        {
            return $"Shopify Variant {variant.sku} (new)";
        }

        public static string LogDescriptor(this ShopifyCustomer customer)
        {
            return $"Shopify Customer {customer.ShopifyCustomerId}";
        }

        public static string LogDescriptor(this Customer customer)
        {
            return $"Acumatica Customer {customer.CustomerID.value}";
        }

        public static string LogDescriptor(this ShopifyOrder shopifyOrder)
        {
            return $"Shopify Order {shopifyOrder.ShopifyOrderNumber} " +
                   $"({shopifyOrder.ShopifyOrderId})";
        }

        public static string LogDescriptor(this ShopifyTransaction transaction)
        {
            return $"Shopify Transaction {transaction.ShopifyKind} " +
                   $"({transaction.ShopifyTransactionId})";
        }

        public static string LogDescriptor(this ShopifyRefund shopifyRefund)
        {
            return $"Shopify Refund {shopifyRefund.ShopifyRefundId} " +
                   $"(Order #{shopifyRefund.ShopifyOrder.ShopifyOrderNumber})";
        }


        public static string LogDescriptor(this AcumaticaStockItem stockItem)
        {
            return $"Acumatica Stock Item {stockItem.ItemId}";
        }

        public static string LogDescriptorItemId(this string itemId)
        {
            return $"Acumatica Stock Item {itemId}";
        }


        public static string LogDescriptor(this AcumaticaSalesOrder salesOrder)
        {
            return $"Acumatica Sales Order {salesOrder.AcumaticaOrderNbr}";
        }

        public static string LogDescriptor(this AcumaticaSoShipment shipmentSalesOrderRef)
        {
            return $"Acumatica Shipment {shipmentSalesOrderRef.AcumaticaShipmentNbr}" +
                   $" - Invoice {shipmentSalesOrderRef.AcumaticaInvoiceNbr}";
        }
    }
}
