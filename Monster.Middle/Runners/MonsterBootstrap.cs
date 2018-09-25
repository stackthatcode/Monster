using Autofac;
using Monster.Acumatica.Config;
using Monster.Middle.Config;
using Monster.Middle.Processes.Inventory;
using Monster.Middle.Processes.Payouts;
using Push.Foundation.Utilities.Autofac;
using Push.Shopify.Http.Credentials;

namespace Monster.Middle.Runners
{
    public class MonsterBootstrap
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

                        process.PullShopifyPayouts(
                            shopifyCredentials,
                            recordsPerPage: payoutConfig.ShopifyRecordsPerPage,
                            maxPages: payoutConfig.ShopifyMaxPages);

                        process.PushAllAcumaticaPayouts(
                            acumaticaCredentials,
                            payoutConfig.ScreenApiUrl);
                    });
            }
        }

    }
}
