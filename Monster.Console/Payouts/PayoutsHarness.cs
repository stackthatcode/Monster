using System;
using Autofac;
using Monster.Acumatica.Http;
using Monster.Middle;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Payouts;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Payout;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;

namespace Monster.ConsoleApp.Payouts
{
    public class PayoutsHarness
    {
        //
        // Payouts - with manual injection of settings
        // (suggested) => this can be run from Console App, or installed to 
        // ... piggyback via a "shim" DLL
        // 
        public static void RunPayoutsWithInjectionOfSettings()
        {
            // TODO - inject your own connection string
            var connectionString = "Server=localhost;Database=Monster;Trusted_Connection=True;";

            // TODO - inject your own Shopify settings
            var shopifyCredentials =
                new PrivateAppCredentials(
                    "ADD API KEY HERE",
                    "ADD API PASSWORD HERE",
                    new ShopDomain("3duniverse.myshopify.com"));

            // TODO - inject your own via new AcumaticaCredentials(); 
            var acumaticaCredentials = new AcumaticaCredentials()
            {
                Username = "ADD USERNAME",
                Password = "ADD PASSWORD",
                CompanyName = "ADD COMPANY NAME",
                InstanceUrl = "ADD URL HERE",
                Branch = "ADD BRANCH HERE",
            };

            // TODO - inject your own config values here            
            var screenApiUrl = "http://localhost/AcuInst2/(W(3))/Soap/BANKIMPORT.asmx";
            var numberOfHeadersToImport = 21; // e.g. 3 weeks

            // TODO - enter your Payout Id here
            var shopifyPayoutId = 123456;


            using (var container = MiddleAutofac.Build())
            using (var scope = container.BeginLifetimeScope())
            {

                var logger = scope.Resolve<IPushLogger>();
                try
                {
                    var process = scope.Resolve<PayoutProcess>();
                    
                    process.Initialize(
                        connectionString,
                        shopifyCredentials,
                        acumaticaCredentials,
                        screenApiUrl);

                    // TODO - use these to control the actual activity
                    process.PullShopifyPayouts();
                    process.PushAllAcumaticaPayouts();

                    process.PullShopifyPayout(shopifyPayoutId);
                    process.PushAcumaticaPayout(shopifyPayoutId);


                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw;
                }
            }
        }


        public static void StressTestDataPopulate()
        {
            using (var container = MiddleAutofac.Build())
            using (var scope = container.BeginLifetimeScope())
            {
                var connectionString = "";

                var persistsContext = scope.Resolve<PersistContext>();
                persistsContext.Initialize(connectionString, 0);

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
