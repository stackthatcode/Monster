using Autofac;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Product;
using Push.Shopify.Api.Transactions;

namespace Monster.ConsoleApp.Shopify
{
    public class TestProcesses
    {
        public static void RetrieveOrderData(
                ILifetimeScope scope, long orderId)
        {
            var orderApi = scope.Resolve<OrderApi>();
            var shopifyOrderJson = orderApi.Retrieve(orderId);

            var orderParent = shopifyOrderJson.DeserializeFromJson<OrderParent>();
            orderParent.order.Initialize();
            var monsterOrderJson = orderParent.SerializeToJson();

            var shopifyTransactionJson = orderApi.RetrieveTransactions(orderId);
            var transactionParent =
                shopifyTransactionJson.DeserializeFromJson<TransactionList>();

            var monsterTransactionJson = transactionParent.SerializeToJson();
        }

        public static void RetrieveProductData(ILifetimeScope scope, long productId)
        {
            var productApi = scope.Resolve<ProductApi>();
            var shopifyOrderJson = productApi.Retrieve(productId);

            var productParent = shopifyOrderJson.DeserializeFromJson<ProductParent>();
            productParent.Initialize();
            var monsterProductJson = productParent.SerializeToJson();

            var inventoryItemIDs = productParent.product.InventoryItemIds;
            var shopifyInventoryLevels 
                    = productApi.RetrieveInventoryLevels(inventoryItemIDs);
        }

        public static void RetrieveLocations(ILifetimeScope scope)
        {
            var productApi = scope.Resolve<InventoryApi>();
            var shopifyLocationJson = productApi.RetrieveLocations();

            var locations =
                shopifyLocationJson
                    .DeserializeFromJson<LocationList>();

            var monsterLocationJson = locations.SerializeToJson();
        }
    }
}
