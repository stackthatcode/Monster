using System;
using Autofac;
using Monster.Middle.Workers.Permutation;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Order;
using Push.Shopify.Config;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ExecuteInLifetimeScope(
                scope =>
                {
                    var factory = scope.Resolve<ApiFactory>();
                    var credentials =
                        ShopifySecuritySettings
                            .FromConfiguration()
                            .MakePrivateAppCredentials();
                    
                    var orderApi = factory.MakeOrderApi(credentials);
                    var result = orderApi.Retrieve(554500751458);

                    var orderParent = result.DeserializeFromJson<OrderParent>();
                    orderParent.order.Initialize();
                    
                    var serializedToJson = orderParent.SerializeToJson();
                });

            Console.WriteLine("Finished - hit any key to exit...");
            Console.ReadKey();
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

