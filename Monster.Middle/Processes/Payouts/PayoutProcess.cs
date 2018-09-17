using Monster.Acumatica.BankImportApi;
using Monster.Acumatica.Config;
using Push.Shopify.Http.Credentials;

namespace Monster.Middle.Processes.Payouts
{
    public class PayoutProcess
    {
        private readonly ShopifyPayoutPullWorker _shopifyPayoutPullWorker;
        private readonly AcumaticaPayoutPushWorkerScreen _acumaticaPayoutPushWorker;

        public PayoutProcess(
                ShopifyPayoutPullWorker shopifyPayoutPullWorker,
                AcumaticaPayoutPushWorkerScreen acumaticaPayoutPushWorker)
        {
            _shopifyPayoutPullWorker = shopifyPayoutPullWorker;
            _acumaticaPayoutPushWorker = acumaticaPayoutPushWorker;
        }

        public void Execute(
                IShopifyCredentials shopifyCredentials,
                AcumaticaCredentials acumaticaCredentials,
                string screenWebSerivceUrl)
        {
            _shopifyPayoutPullWorker
                .ImportPayoutHeaders(shopifyCredentials, maxPages: 1, recordsPerPage: 5);
            _shopifyPayoutPullWorker
                .ImportIncompletePayoutTransactions(shopifyCredentials);
            _shopifyPayoutPullWorker
                .GenerateBalancingSummaries(5);
            
            //
            // TODO - add screen to  to Autofac registration
            //
            using (var screen = new Screen())
            {
                _acumaticaPayoutPushWorker
                    .BeginSession(
                        screen,
                        screenWebSerivceUrl,
                        acumaticaCredentials);

                _acumaticaPayoutPushWorker
                    .WritePayoutToAcumatica(
                        screen, 19276529764);

                _acumaticaPayoutPushWorker
                    .WritePayoutToAcumatica(
                        screen, 19248185444);

                screen.Logout();
            }
        }
    }
}
