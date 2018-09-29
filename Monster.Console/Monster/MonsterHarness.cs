﻿using Autofac;
using Monster.Acumatica.Http;
using Monster.Middle;
using Monster.Middle.Config;
using Monster.Middle.Processes.Inventory;
using Push.Foundation.Utilities.Autofac;
using Push.Shopify.Http.Credentials;


namespace Monster.ConsoleApp.Monster
{
    public class MonsterHarness
    {
        public const string DefaultLoggerName = "Monster.Payouts";

        //
        // NOTE => The security settings will have to be inferred by the task identifiers
        // ... and the configuration data pulled on a per-tenant basis from 
        // ... secure storage. For now, we stub away!
        //

        public static IContainer 
                ContainerFactory(PayoutConfig payoutConfig)
        {
            return MiddleAutofac.Build(
                connStringOverride: payoutConfig.ConnectionString,
                loggerName: DefaultLoggerName);
        }

        public static void RunPayoutsEndToEnd(
            PrivateAppCredentials shopifyCredentials,
            AcumaticaCredentials acumaticaCredentials,
            PayoutConfig payoutConfig)
        {
            using (var container = ContainerFactory(payoutConfig))
            {
                container.RunInLifetimeScope(
                    scope =>
                    {
                        var process = scope.Resolve<InventoryWorker>();

                        //process.PullShopifyPayouts(
                        //    shopifyCredentials,
                        //    recordsPerPage: payoutConfig.ShopifyRecordsPerPage,
                        //    maxPages: payoutConfig.ShopifyMaxPages);

                        //process.PushAllAcumaticaPayouts(
                        //    acumaticaCredentials,
                        //    payoutConfig.ScreenApiUrl);
                    });
            }
        }

    }
}
