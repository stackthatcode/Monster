using Autofac;
using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Payouts;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;


namespace Monster.ConsoleApp.Monster
{
    public class PayoutsHarness
    {
        public const int DummyCompanyId = 1;

        // If you're going to customize Shopify's behavior, it goes here
        public static void Initialize(
                ILifetimeScope scope,
                string connectionString,
                PrivateAppCredentials shopifyCredentials,
                AcumaticaCredentials acumaticaCredentials,
                string bankImportScreenUrl)
        {
            var persistContext = scope.Resolve<PersistContext>();            
            persistContext.Initialize(connectionString, DummyCompanyId);

            var shopifyContext = scope.Resolve<ShopifyHttpContext>();
            shopifyContext.Initialize(shopifyCredentials);

            var payoutConfig = scope.Resolve<PayoutConfig>();
            payoutConfig.ScreenApiUrl = bankImportScreenUrl;
        }
        

        public static void RunPayoutsEndToEnd(ILifetimeScope scope)
        {
            var process = scope.Resolve<PayoutProcess>();

            process.PullShopifyPayouts();
            process.BeginAcumaticaSession();
            process.PushAllAcumaticaPayouts();
            process.EndAcumaticaSession();
        }

        public static void PullPayoutFromShopify(
                    ILifetimeScope scope, long shopifyPayoutId)
        {
            var process = scope.Resolve<PayoutProcess>();

            process.BeginAcumaticaSession();
            process.PullShopifyPayout(shopifyPayoutId);                        
            process.EndAcumaticaSession();
        }

        public static void PushToAcumatica(
                    ILifetimeScope scope, long shopifyPayoutId)
        {
            var process = scope.Resolve<PayoutProcess>();

            process.BeginAcumaticaSession();
            process.PushAcumaticaPayout(shopifyPayoutId);
            process.EndAcumaticaSession();
        }
    }
}

