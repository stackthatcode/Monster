using System;
using System.Linq;
using Autofac;
using Monster.Acumatica.Http;
using Monster.ConsoleApp.TestJson;
using Monster.ConsoleApp._3duStuff;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Payout;
using Push.Shopify.Api.Product;
using Push.Shopify.Api.Transaction;
using Push.Shopify.Config;
using Push.Shopify.HttpClient.Credentials;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // DeserializeJson<BalanceTransactionList>("3duPayouts20180813.json");
            // DeserializeJson<TransactionList>("3duPayPalTransactions.json");
            // ExecuteInLifetimeScope(scope => RetrieveOrderData(scope));
            // ExecuteInLifetimeScope(scope => RetrieveProductData(scope, 1403130544226));
            // ExecuteInLifetimeScope(scope => RetrieveLocations(scope));
            // ExecuteInLifetimeScope(scope => RetrievePayoutDta(scope));            
            // ExecuteInLifetimeScope(scope => Metaplay.UpdateMetadata(scope));

            ExecuteInLifetimeScope(scope => RetrieveAcumaticaItemClass(scope));

            Console.WriteLine("Finished - hit any key to exit...");
            Console.ReadKey();
        }

        public static IShopifyCredentials CredentialsFactory()
        {
            return ShopifySecuritySettings
                    .FromConfiguration()
                    .MakePrivateAppCredentials();
        }

        static void RetrieveOrderData(ILifetimeScope scope, long orderId)
        {
            var factory = scope.Resolve<ApiFactory>();
            var credentials = CredentialsFactory();

            var orderApi = factory.MakeOrderApi(credentials);
            var shopifyOrderJson = orderApi.Retrieve(orderId);

            var orderParent = shopifyOrderJson.DeserializeFromJson<OrderParent>();
            orderParent.order.Initialize();
            var monsterOrderJson = orderParent.SerializeToJson();

            var shopifyTransactionJson = orderApi.RetrieveTransactions(orderId);
            var transactionParent =
                    shopifyTransactionJson.DeserializeFromJson<TransactionList>();

            var monsterTransactionJson = transactionParent.SerializeToJson();
        }
        
        static void RetrieveProductData(ILifetimeScope scope, long productId)
        {
            var factory = scope.Resolve<ApiFactory>();
            var credentials = CredentialsFactory();

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
            var credentials = CredentialsFactory();
            
            var productApi = factory.MakeProductApi(credentials);
            var shopifyLocationJson = productApi.RetrieveLocations();

            var locations = shopifyLocationJson.DeserializeFromJson<LocationList>();
            var monsterLocationJson = locations.SerializeToJson();
        }

        static void RetrievePayoutDta(ILifetimeScope scope)
        {
            var factory = scope.Resolve<ApiFactory>();
            var credentials = CredentialsFactory();
            var payoutApi = factory.MakePayoutApi(credentials);

            //var date = new DateTimeOffset(2018, 8, 8, 0, 0, 0, new TimeSpan(0, 0, 0));

            var shopifyPayoutJson = payoutApi.RetrievePayouts();
            var monsterPayout = shopifyPayoutJson.DeserializeFromJson<PayoutList>();
            var firstPayoutId = monsterPayout.payouts.First().id;

            var shopifyPayoutDetailJson = payoutApi.RetrievePayoutDetail(firstPayoutId);
            var monsterPayoutDetail = shopifyPayoutDetailJson.DeserializeFromJson<PayoutDetail>();
        }
        
        static void DeserializeJson<T>(string inputJsonFile)
        {
            var json = TestLoader.GimmeJson(inputJsonFile);
            var deserializedObject = json.DeserializeFromJson<T>();
            var reserializedJson = deserializedObject.SerializeToJson();
            Console.WriteLine(reserializedJson);
        }

        static void RetrieveAcumaticaItemClass(ILifetimeScope scope)
        {
            var repository = scope.Resolve<SpikeRepository>();
            repository.RetrieveSession();
            var results = repository.RetrieveItemClass();
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

