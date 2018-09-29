using System;
using Autofac;
using Monster.Acumatica.Config;
using Monster.ConsoleApp.Shopify;
using Monster.Middle;
using Monster.Middle.Config;
using Monster.Middle.Sql;
using Monster.Middle.Sql.Multitenant;
using Push.Foundation.Utilities.Autofac;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Payout;
using Push.Shopify.Config;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;
using PayoutHarness = Monster.ConsoleApp.Monster.PayoutHarness;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //StressTestDataPopulate();
            //RunPayouts();

            RunMetafieldCopy();

            Console.WriteLine("Finished - hit any key to exit...");
            Console.ReadKey();
        }

        public static void RunMetafieldCopy()
        {
            var credentials = ShopifyPrivateAppCredentialsFromConfig();


            Console.WriteLine("Copy 3DU_Automation Metafields");
            Console.WriteLine("****");

            Console.WriteLine("Enter Source Product ID");
            var sourceProductId = Console.ReadLine().ToLong();

            Console.WriteLine("Enter Target Product ID");
            var targetProductId = Console.ReadLine().ToLong();
            
            Console.WriteLine(Environment.NewLine + "Ok, running...");
            
            using (var container = MiddleAutofac.Build())
            {
                container.RunInLifetimeScope(
                    scope => 
                        MetafieldHarness.CopyShoppingFeedMetadata(
                            scope,
                            credentials,
                            sourceProductId,
                            targetProductId,
                            "3DU_AUTOMATION"));
            }

            Console.WriteLine("FIN");
            Console.ReadKey();
        }


        public static void RunPayouts()
        {
            // TODO - inject your own via new PrivateAppCredentials();
            var shopifyCredentials =
                new PrivateAppCredentials(
                    "API KEY HERE",
                    "API PASSWORD HERE",
                    new ShopDomain("3duniverse.myshopify.com"));
            
            // TODO - inject your own via new AcumaticaCredentials(); 
            var acumaticaCredentials = new AcumaticaCredentials();
            
            // TODO - inject your own config values here
            var payoutConfig = PayoutConfigFactory();
            

            // *** OPTIONS FOR RUNNING - choose one!
            
            // #1 - this will pull from Shopify everything as specified in Payout Config
            // ... and push *every* possible Shopify transaction
            //PayoutBootstrap.RunPayoutsEndToEnd(
            //        shopifyCredentials, acumaticaCredentials, payoutConfig);

            // #2 - this will search Shopify and pull a solitary Payout
            // ... and all its Transactions from Shopify
            //var payoutId = 1234;
            //PayoutBootstrap.PullFromShopify(
            //        shopifyCredentials, payoutConfig, payoutId);

            // #3 - this will only load the header and transactions from the
            // ... single Payout identified into Acumatica
            var payoutId2 = 11111111111;
            PayoutHarness.PushToAcumatica(
                acumaticaCredentials, payoutConfig, payoutId2);

        }


        public static PrivateAppCredentials 
                        ShopifyPrivateAppCredentialsFromConfig()
        {
            return ShopifyCredentials
                .FromConfiguration()
                .MakePrivateAppCredentials();
        }

        public static AcumaticaCredentials 
                        AcumaticaCredentialsFromConfig()
        {
            var config = AcumaticaCredentialsConfig.Settings;
            return new AcumaticaCredentials(config);
        }

        public static PayoutConfig PayoutConfigFactory()
        {
            var payoutConfig = new PayoutConfig
            {
                // TODO - inject your own connection string
                ConnectionString =
                    "Server=localhost;Database=Monster;Trusted_Connection=True;",

                // TODO - inject your own Screen URL
                ScreenApiUrl =
                    "http://localhost/AcuInst2/(W(3))/Soap/BANKIMPORT.asmx",
            };
            return payoutConfig;
        }



        public static void StressTestDataPopulate()
        {
            var payoutConfig = PayoutConfigFactory();
            using (var container =
                        MiddleAutofac.Build(
                            connStringOverride: payoutConfig.ConnectionString,
                            loggerName: "Monster.Payouts"))
            using (var scope = container.BeginLifetimeScope())
            {
                var repository = scope.Resolve<PayoutPersistRepository>();
                var logger = scope.Resolve<IPushLogger>();

                var shopifyPayout = new Payout()
                {
                    id = 1000000000,
                    date = new DateTime(2018, 09, 18),
                    currency = "USD",
                    status = "paid"
                };

                var payout_id = 11111111111;

                var payout = new UsrShopifyPayout()
                {
                    ShopifyPayoutId = 11111111111,
                    ShopifyLastStatus = "paid",
                    CreatedDate = DateTime.UtcNow,
                    AllShopifyTransDownloaded = true,
                    Json = shopifyPayout.SerializeToJson(),
                };

                repository.InsertPayoutHeader(payout);

                var counter = 0;

                var rand = new Random();

                while (counter++ < 10000)
                {
                    if (counter % 100 == 0)
                    {
                        logger.Info($"Counter: {counter}");
                    }

                    var amount = rand.Next(100, 1000);
                    var fee = rand.Next(10, 100);

                    var shopifyTransaction = new PayoutTransaction
                    {
                        payout_id = payout_id,
                        amount = amount,
                        currency = "USD",
                        payout_status = "paid",
                        type = "charge",
                        fee = fee,
                        net = amount - fee,
                        source_id = 200000000 + counter,
                        id = 300000000 + counter,
                        source_order_transaction_id = 400000000 + counter,
                        source_order_id = 500000000 + counter,
                    };

                    var transaction = new UsrShopifyPayoutTransaction()
                    {
                        ShopifyPayoutId = payout_id,
                        CreatedDate = DateTime.UtcNow,
                        ShopifyPayoutTransId = shopifyTransaction.id,
                        Type = shopifyTransaction.type,
                        Json = shopifyTransaction.SerializeToJson(),
                    };

                    repository.InsertPayoutTransaction(transaction);
                }
            }
        }
    }
}

