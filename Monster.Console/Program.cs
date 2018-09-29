using System;
using Autofac;
using Monster.ConsoleApp.Monster;
using Monster.ConsoleApp.Shopify;
using Monster.Middle;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Payout;
using Push.Shopify.Config;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // *** Uncomment a process flow to execute...

            // Payouts runs
            //PayoutsRunner.RunPayoutsWithInjectionOfSettings();
            //PayoutsRunner.StressTestDataPopulate();
            

            // Shopify test runs
            RunShopifyMetafieldCopy();


            // Monster test runs


            Console.WriteLine("Finished - hit any key to exit...");
            Console.ReadKey();
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

