using Autofac;
using Monster.Acumatica.Config;
using Monster.Middle;
using Monster.Middle.Config;
using Monster.Middle.Processes.Payouts;
using Push.Foundation.Utilities.Autofac;
using Push.Shopify.Http.Credentials;

namespace Monster.ConsoleApp.Monster
{
    public class PayoutHarness
    {
        public const string DefaultLoggerName = "Monster.Payouts";

        public static IContainer ContainerFactory(PayoutConfig payoutConfig)
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
                        var process = scope.Resolve<PayoutProcess>();

                        process.PullShopifyPayouts(
                            recordsPerPage: payoutConfig.ShopifyRecordsPerPage,
                            maxPages: payoutConfig.ShopifyMaxPages);

                        process.PushAllAcumaticaPayouts(
                            acumaticaCredentials,
                            payoutConfig.ScreenApiUrl);
                    });
            }
        }

        public static void PullFromShopify(
                PrivateAppCredentials shopifyCredentials,
                PayoutConfig payoutConfig, 
                long shopifyPayoutId)
        {
            using (var container = ContainerFactory(payoutConfig))
            {
                container.RunInLifetimeScope(
                    scope =>
                    {
                        var process = scope.Resolve<PayoutProcess>();

                        process.PullShopifyPayouts(
                            recordsPerPage: payoutConfig.ShopifyRecordsPerPage,
                            maxPages: payoutConfig.ShopifyMaxPages,
                            shopifyPayoutId: shopifyPayoutId);                        
                    });
            }
        }

        public static void PushToAcumatica(
                    AcumaticaCredentials acumaticaCredentials,
                    PayoutConfig payoutConfig,
                    long shopifyPayoutId)
        {
            using (var container = ContainerFactory(payoutConfig))
            {
                container.RunInLifetimeScope(
                    scope =>
                    {
                        var process = scope.Resolve<PayoutProcess>();
                        
                        process.PushAcumaticaPayout(
                                    acumaticaCredentials,
                                    payoutConfig.ScreenApiUrl,
                                    shopifyPayoutId);
                    });
            }
        }
    }
}

