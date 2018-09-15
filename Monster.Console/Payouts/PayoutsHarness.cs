using Autofac;
using Monster.Acumatica.Config;
using Monster.Middle.Workers;
using Push.Shopify.Config;
using Push.Shopify.Http.Credentials;

namespace Monster.ConsoleApp.Payouts
{
    public class PayoutsHarness
    {
        public static IShopifyCredentials ShopifyCredentialsFactory()
        {
            return ShopifySecuritySettings
                .FromConfiguration()
                .MakePrivateAppCredentials();
        }

        public static AcumaticaCredentials AcumaticaCredentialsFactory()
        {
            var config = AcumaticaCredentialsConfig.Settings;
            return new AcumaticaCredentials(config);
        }


        public static void PullPayoutsIntoAcumatica(ILifetimeScope scope)
        {
            //var payoutPull = scope.Resolve<ShopifyPayoutPull>();
            //var credentials = ShopifyCredentialsFactory();

            //payoutPull.ImportPayoutHeaders(credentials, maxPages: 1, recordsPerPage: 10);
            //payoutPull.ImportIncompletePayoutTransactions(credentials);
            //payoutPull.GenerateBalancingSummaries(10);
            

            var acumaticaPush = scope.Resolve<AcumaticaPayoutPush>();
            var acucredentials = AcumaticaCredentialsFactory();
            acumaticaPush
                .WritePayoutHeaderToAcumatica(
                    acucredentials, 19248185444, "102000");
        }
    }
}
