using System;
using Autofac;
using Monster.Acumatica.Config;
using Monster.Middle.Processes.Payouts;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Config;
using Push.Shopify.Http.Credentials;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            RunShopifyToAcumaticaPayouts();            
            
            Console.WriteLine("Finished - hit any key to exit...");
            Console.ReadKey();
        }


        public static void RunShopifyToAcumaticaPayouts()
        {
            // TODO - inject Connection String override here if you're not
            // ... going to use the config file

            var connectionString = 
                "Server=localhost;Database=Monster;Trusted_Connection=True;";

            using (var container =
                    ConsoleAutofac.Build(
                        connectionStringOverride: connectionString))

            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<IPushLogger>();
                try
                {
                    // TODO - inject your own PrivateAppCredentials 
                    var shopifyCredentials = ShopifyCredentialsFactory();

                    // TODO - inject your own AcumaticaCredentials 
                    var acumaticaCredentials = AcumaticaCredentialsFactory();

                    var process = scope.Resolve<PayoutProcess>();
                    process.Execute(shopifyCredentials, acumaticaCredentials);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw;
                }
            }
        }
        
        public static IShopifyCredentials ShopifyCredentialsFactory()
        {
            return ShopifySecuritySettings
                .FromConfiguration()
                .MakePrivateAppCredentials();
        }

        public static AcumaticaCredentials AcumaticaCredentialsFactory()
        {
            var config = AcumaticaCredentialsConfig.Settings;
            return new AcumaticaCredentials(config);
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

