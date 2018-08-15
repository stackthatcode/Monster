using System;
using System.IO;
using Autofac;
using Monster.Middle.Workers.Permutation;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Transaction;
using Push.Shopify.Config;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // ExecuteInLifetimeScope(scope => RetrieveOrderData(scope));

            DeserializePayPalTransaction();

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
        
        static void GeneratePermutations()
        {
            ExecuteInLifetimeScope(
                scope =>
                {
                    var worker = scope.Resolve<PermutationWorker>();
                    worker.Do();
                });
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

