using System;
using System.IO;
using Autofac;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Product;
using Push.Shopify.Api.Transaction;
using Push.Shopify.Config;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // DeserializePayPalTransaction();
            // ExecuteInLifetimeScope(scope => RetrieveOrderData(scope));
            ExecuteInLifetimeScope(scope => RetrieveProductData(scope, 1403130544226));
            //ExecuteInLifetimeScope(scope => RetrieveLocations(scope));

            Console.WriteLine("Finished - hit any key to exit...");
            Console.ReadKey();
        }


        static void RetrieveOrderData(ILifetimeScope scope, long orderId)
        {
            var factory = scope.Resolve<ApiFactory>();
            var credentials =
                ShopifySecuritySettings
                    .FromConfiguration()
                    .MakePrivateAppCredentials();

            var orderApi = factory.MakeOrderApi(credentials);
            var shopifyOrderJson = orderApi.Retrieve(orderId);

            var orderParent = shopifyOrderJson.DeserializeFromJson<OrderParent>();
            orderParent.order.Initialize();
            var monsterOrderJson = orderParent.SerializeToJson();

            var shopifyTransactionJson = orderApi.RetrieveTransactions(orderId);
            var transactionParent =
                    shopifyTransactionJson.DeserializeFromJson<TransactionRoot>();

            var monsterTransactionJson = transactionParent.SerializeToJson();
        }


        static void RetrieveProductData(ILifetimeScope scope, long productId)
        {
            var factory = scope.Resolve<ApiFactory>();
            var credentials =
                ShopifySecuritySettings
                    .FromConfiguration()
                    .MakePrivateAppCredentials();

            var productApi = factory.MakeProductApi(credentials);
            var shopifyOrderJson = productApi.Retrieve(productId);

            var productParent = shopifyOrderJson.DeserializeFromJson<ProductParent>();
            productParent.Initialize();
            var monsterProductJson = productParent.SerializeToJson();

            var inventoryItemIDs = productParent.product.InventoryItemIds;
            var shopifyInventoryLevels = productApi.RetrieveInventoryLevels(inventoryItemIDs);
        }

        static void RetrieveLocations(ILifetimeScope scope)
        {
            var factory = scope.Resolve<ApiFactory>();
            var credentials =
                ShopifySecuritySettings
                    .FromConfiguration()
                    .MakePrivateAppCredentials();

            var productApi = factory.MakeProductApi(credentials);
            var shopifyLocationJson = productApi.RetrieveLocations();

            var locations = shopifyLocationJson.DeserializeFromJson<LocationList>();
            var monsterLocationJson = locations.SerializeToJson();
        }



        static void DeserializePayPalTransaction()
        {
            var data = File.ReadAllText(@".\TestJson\3duPayPalTransactions.json");
            var transaction = data.DeserializeFromJson<TransactionRoot>();
            var jsonAgain = transaction.SerializeToJson();
        }

        static void ExecuteInLifetimeScope(Action<ILifetimeScope> action)
        {
            using (var container = ConsoleAutofac.Build(false))
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<IPushLogger>();
                try
                {
                    action(scope);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw;
                }
            }
        }

    }
}

