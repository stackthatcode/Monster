using Monster.Acumatica.BankImportApi;
using Monster.Acumatica.Config;
using Monster.Middle.EF;
using Push.Shopify.Http.Credentials;

namespace Monster.Middle.Processes.Payouts
{
    public class PayoutProcess
    {
        private readonly ShopifyPayoutPullWorker _shopifyPayoutPullWorker;
        private readonly AcumaticaPayoutPushWorkerScreen _acumaticaPayoutPushWorker;
        private readonly PayoutImportRepository _persistenceRepository;

        public PayoutProcess(
                ShopifyPayoutPullWorker shopifyPayoutPullWorker,
                AcumaticaPayoutPushWorkerScreen acumaticaPayoutPushWorker, 
                PayoutImportRepository persistenceRepository)
        {
            _shopifyPayoutPullWorker = shopifyPayoutPullWorker;
            _acumaticaPayoutPushWorker = acumaticaPayoutPushWorker;
            _persistenceRepository = persistenceRepository;
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
            foreach (var payout in
                _persistenceRepository.RetrieveNotYetUploadedPayouts())
            {
                using (var screen = new Screen())
                {
                    _acumaticaPayoutPushWorker
                        .BeginSession(
                            screen,
                            screenWebSerivceUrl,
                            acumaticaCredentials);

                    _acumaticaPayoutPushWorker
                        .WritePayoutToAcumatica(
                            screen, payout.ShopifyPayoutId);

                    screen.Logout();
                }
            }
        }
    }
}
