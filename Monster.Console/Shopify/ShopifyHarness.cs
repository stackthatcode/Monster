using System;
using Autofac;
using Monster.Middle;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Config;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;

namespace Monster.ConsoleApp.Shopify
{
    public class ShopifyHarness
    {
        public static 
                void SetCredentialsAndExecute(
                        IContainer container, 
                        IShopifyCredentials credentials,
                        Action<ILifetimeScope> action)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                //
                // *** This takes the passed credentials and injects them
                //

                var shopifyHttpContext = scope.Resolve<ShopifyHttpContext>();
                shopifyHttpContext.Initialize(credentials);

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



        public static void RunShopifyMetafieldCopy()
        {
            // Solicit information from user
            Console.WriteLine("Copy 3DU_Automation Metafields" + Environment.NewLine);
            Console.WriteLine("Enter Source Product ID:");
            var sourceProductId = Console.ReadLine().ToLong();
            Console.WriteLine("Enter Target Product ID:");
            var targetProductId = Console.ReadLine().ToLong();
            Console.WriteLine(Environment.NewLine + "Ok, running...");


            // Create Autofac IContainer
            using (var container = MiddleAutofac.Build())
            {
                // TODO - Get credentials from config file
                var credentials
                    = ShopifyCredentialsConfig.Settings.ToPrivateAppCredentials();

                ShopifyHarness
                    .SetCredentialsAndExecute(
                        container,
                        credentials,
                        scope =>
                        {
                            MetafieldProcesses.CopyShoppingFeedMetadata(
                                scope,
                                sourceProductId,
                                targetProductId,
                                "3DU_AUTOMATION");
                        });
            }

            Console.WriteLine("FIN");
            Console.ReadKey();
        }


    }
}

