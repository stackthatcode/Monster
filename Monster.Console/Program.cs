using System;
using Autofac;
using Monster.Middle.Workers;
using Monster.Middle.Workers.Permutation;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Product;
using Push.Shopify.Config;
using Push.Shopify.Credentials;


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
                    
                    var productApi = factory.MakeProductApi(credentials);
                    var filter = new ProductFilter()
                    {
                        UpdatedAtMinUtc = DateTime.Today.AddDays(-7)
                    };
                    var result = productApi.RetrieveCount(filter);
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
