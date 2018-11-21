using System;
using Autofac;
using Monster.Acumatica.Http;
using Monster.Middle;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Shopify;
using Monster.Middle.Processes.Payouts;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Payout;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;

namespace Monster.ConsoleApp.Payouts
{
    public class PayoutsHarness
    {
        public static void RunPayoutsByTenant()
        {
            using (var container = MiddleAutofac.Build())
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<IPushLogger>();
                try
                {
                    var tenantId = new Guid("1ADACC65-43EB-4083-9A14-1D3601F52328");

                    // Get load the Installation
                    var tenantContext = scope.Resolve<TenantContext>();
                    tenantContext.Initialize(tenantId);

                    // Get the Installation's 
                    var tenantContextRepository = scope.Resolve<TenantRepository>();
                    var credentials = tenantContextRepository.RetrieveAcumaticaCredentials();

                    // Load Payout configuration into Bank Import Service
                    var payoutConfig = new PayoutConfig
                    {
                        Credentials = credentials,
                        ScreenApiUrl = "http://localhost/AcuInst2/Soap/BANKIMPORT.asmx"
                    };
                    var bankImportService = scope.Resolve<BankImportService>();
                    bankImportService.Initialize(payoutConfig);

                    // Get the Payouts process
                    var process = scope.Resolve<PayoutProcess>();

                    // TODO - use these to control the actual activity
                    process.PullShopifyPayouts();
                    //process.PushAllAcumaticaPayouts();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw;
                }
            }
        }


        //
        // Payouts - with manual injection of settings
        // (suggested) => this can be run from Console App, or installed to 
        // ... piggyback via a "shim" DLL
        // 
        public static void RunPayoutsWithInjectionOfSettings()
        {

            using (var container = MiddleAutofac.Build())
            using (var scope = container.BeginLifetimeScope())
            {

                var logger = scope.Resolve<IPushLogger>();
                try
                {
                    // TODO - inject your own connection string
                    var connectionString = "Server=localhost;Database=MonsterSys;Trusted_Connection=True;";


                    var persistContext = scope.Resolve<PersistContext>();
                    persistContext.Initialize(connectionString, 1);

                    // TODO - inject your own Shopify settings
                    var shopifyCredentials =
                        new PrivateAppCredentials(
                            "ADD API KEY HERE",
                            "ADD API PASSWORD HERE",
                            new ShopDomain("3duniverse.myshopify.com"));

                    var shopifyHttpContext = scope.Resolve<ShopifyHttpContext>();
                    shopifyHttpContext.Initialize(shopifyCredentials);


                    // TODO - inject your own via new AcumaticaCredentials(); 
                    var acumaticaCredentials = new AcumaticaCredentials()
                    {
                        Username = "ADD USERNAME",
                        Password = "ADD PASSWORD",
                        CompanyName = "ADD COMPANY NAME",
                        InstanceUrl = "ADD URL HERE",
                        Branch = "ADD BRANCH HERE",
                    };

                    var screenApiUrl = "http://localhost/AcuInst2/(W(3))/Soap/BANKIMPORT.asmx";
                    
                    // TODO - inject your own config values here            
                    var payoutConfig = new PayoutConfig
                    {
                        Credentials = acumaticaCredentials,
                        ScreenApiUrl = screenApiUrl,
                    };
                    
                    var bankImportService = scope.Resolve<BankImportService>();
                    bankImportService.Initialize(payoutConfig);


                    // TODO - use these to control the actual activity
                    var process = scope.Resolve<PayoutProcess>();

                    process.PullShopifyPayouts();
                    //process.PushAllAcumaticaPayouts();

                    // TODO - enter your Payout Id here
                    var shopifyPayoutId = 123456;
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

                var repository = scope.Resolve<ShopifyPayoutRepository>();
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
                    AllTransDownloaded = true,
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
