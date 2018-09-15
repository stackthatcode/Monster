using Autofac;
using Monster.Middle.Workers;
using Push.Shopify.Config;
using Push.Shopify.Http.Credentials;

namespace Monster.ConsoleApp
{
    public class MonsterHarness
    {
        public static IShopifyCredentials CredentialsFactory()
        {
            return ShopifySecuritySettings
                .FromConfiguration()
                .MakePrivateAppCredentials();
        }

        public static void PullPayoutsIntoAcumatica(ILifetimeScope scope)
        {
            var fetcher = scope.Resolve<ShopifyPayoutFetcher>();
            var credentials = CredentialsFactory();

            //fetcher.ImportPayoutHeaders(credentials, maxPages: 1, recordsPerPage: 10);
            //fetcher.ImportIncompletePayoutTransactions(credentials);
            fetcher.GenerateBalancingSummaries(10);
        }
    }
}
