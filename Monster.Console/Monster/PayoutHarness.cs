using Autofac;
using Monster.Acumatica.Http;
using Monster.Middle;
using Monster.Middle.Config;
using Monster.Middle.Processes.Payouts;
using Monster.Middle.Services;
using Push.Shopify.Http.Credentials;


namespace Monster.ConsoleApp.Monster
{
    public class PayoutHarness
    {
        // If you're going to customize Shopify's behavior, it goes here
        public static void Initialize(
                ILifetimeScope scope,
                PrivateAppCredentials shopifyCredentials,
                AcumaticaCredentials acumaticaCredentials,
                string connectionString,
                string screenUrl)
        {
            var tenantContextLoader = scope.Resolve<TenantContextLoader>();
            const int dummyCompanyId = 1;

            tenantContextLoader.Initialize(
                connectionString, 
                dummyCompanyId,
                shopifyCredentials,
                acumaticaCredentials);

            var payoutConfig = scope.Resolve<PayoutConfig>();
            payoutConfig.ScreenApiUrl = screenUrl;
        }


        public static void RunPayoutsEndToEnd(
                ILifetimeScope scope, PayoutConfig payoutConfig)
        {
            var process = scope.Resolve<PayoutProcess>();
            process.PullShopifyPayouts();
            process.PushAllAcumaticaPayouts();
        }

        public static void PullFromShopify(
                ILifetimeScope scope, long shopifyPayoutId)
        {
            var process = scope.Resolve<PayoutProcess>();
            process.PullShopifyPayouts(shopifyPayoutId);                        
        }

        public static void PushToAcumatica(
                ILifetimeScope scope, long shopifyPayoutId)
        {
            var process = scope.Resolve<PayoutProcess>();
            process.PushAcumaticaPayout(shopifyPayoutId);
        }
    }
}

