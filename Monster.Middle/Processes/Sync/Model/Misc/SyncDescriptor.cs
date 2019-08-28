using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Misc
{
    public static class SyncDescriptor
    {
        public const string UpdateShopifyPrice = "Update Shopify Price";
        public const string UpdateShopifyInventory = "Update Shopify Inventory";
        public const string CreateShopifyFulfillment = "Create Shopify Fulfillment";

        public const string CreateStockItem = "Create Acumatica Stock Item";
        public const string CreateInventoryReceipt = "Create Acumatica Inventory Receipt";
        
        public const string CreateAcumaticaCustomer = "Create Acumatica Customer";
        public const string UpdateAcumaticaCustomer = "Update Acumatica Customer";
        public const string CreateAcumaticaSalesOrder = "Create Acumatica Sales Order";
        public const string UpdateAcumaticaSalesOrderTaxes = "Update Acumatica Sales Order Taxes";

        public const string CreateAcumaticaPayment = "Create Acumatica Payment";

        public const string CreateAcumaticaCreditMemo = "Create Acumatica Credit Memo";
        public const string UpdateAcumaticaCreditMemoTaxes = "Update Acumatica Credit Memo Taxes";
        public const string DetectAcumaticaCreditMemoInvoice = "Detect Acumatica Credit Memo Invoice";
        public const string CreateAcumaticaCreditMemoInvoice = "Create Acumatica Credit Memo Invoice";
        public const string ReleaseAcumaticaCreditMemoInvoice = "Release Acumatica Credit Memo Invoice";


        public static string ShopifyProduct(long shopifyProductId)
        {
            return $"Shopify Product {shopifyProductId}";
        }

        public static string ShopifyVariant(ShopifyVariant variant)
        {
            return $"Shopify Variant {variant.ShopifySku} ({variant.ShopifyVariantId})";
        }
        
        public static string ShopifyCustomer(UsrShopifyCustomer customer)
        {
            return $"Shopify Customer {customer.ShopifyCustomerId}";
        }

        public static string ShopifyOrder(UsrShopifyOrder shopifyOrder)
        {
            return $"Shopify Order {shopifyOrder.ShopifyOrderNumber} ({shopifyOrder.ShopifyOrderId})";
        }

        public static string ShopifyTransaction(UsrShopifyTransaction transaction)
        {
            return $"Shopify Transaction {transaction.ShopifyKind} ({transaction.ShopifyTransactionId})";
        }

        public static string ShopifyRefund(UsrShopifyRefund shopifyRefund)
        {
            return $"Shopify Refund {shopifyRefund.ShopifyRefundId} " + 
                   $"(Order #{shopifyRefund.UsrShopifyOrder.ShopifyOrderNumber})";
        }


        public static string AcumaticaStockItem(AcumaticaStockItem stockItem)
        {
            return $"Acumatica Stock Item {stockItem.ItemId}";
        }

        public static string AcumaticaSalesOrder(UsrAcumaticaSalesOrder salesOrder)
        {
            return $"Acumatica Sales Order {salesOrder.AcumaticaOrderNbr}";
        }

        public static string 
                AcumaticaShipmentSalesOrderRef(
                    UsrAcumaticaShipmentSalesOrderRef shipmentSalesOrderRef)
        {
            return $"Acumatica Shipment {shipmentSalesOrderRef.AcumaticaShipmentNbr} " +
                $"for Sales Order {shipmentSalesOrderRef.AcumaticaOrderNbr}";
        }

    }
}
