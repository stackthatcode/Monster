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

        public void PullShopifyPayouts(
                        IShopifyCredentials shopifyCredentials,
                        int recordsPerPage = 14, 
                        int maxPages = 1,
                        bool includeTransactions = true,
                        long? shopifyPayoutId = null)
        {
            _shopifyPayoutPullWorker
                .ImportPayoutHeaders(
                    shopifyCredentials, 
                    maxPages, 
                    recordsPerPage,
                    shopifyPayoutId);

            if (includeTransactions)
            {
                if (shopifyPayoutId.HasValue)
                {
                    _shopifyPayoutPullWorker
                        .ImportPayoutTransactions(
                            credentials:shopifyCredentials, 
                            payoutId:shopifyPayoutId.Value);
                }
                else
                {
                    _shopifyPayoutPullWorker
                        .ImportIncompletePayoutTransactions(
                            shopifyCredentials);
                }
            }

            // 
            // Entirely optional self-analysis
            //
            //_shopifyPayoutPullWorker
            //    .LogBalancingSummaries(5);
        }

        public void PushAllAcumaticaPayouts(               
                AcumaticaCredentials acumaticaCredentials,
                string screenWebSerivceUrl)
        {
            _acumaticaPayoutPushWorker
                .BeginSession(
                    screenWebSerivceUrl,
                    acumaticaCredentials);

            foreach (var payout in
                _persistenceRepository.RetrieveNotYetUploadedPayouts())
            {
                _acumaticaPayoutPushWorker
                    .WritePayoutToAcumatica(payout.ShopifyPayoutId);
            }

            _acumaticaPayoutPushWorker.EndSession();
        }

        public void PushAcumaticaPayout(
                AcumaticaCredentials acumaticaCredentials,
                string screenWebSerivceUrl, 
                long shopifyPayoutId)
        {
            _acumaticaPayoutPushWorker
                .BeginSession(
                    screenWebSerivceUrl,
                    acumaticaCredentials);

            _acumaticaPayoutPushWorker
                .WritePayoutToAcumatica(shopifyPayoutId);

            _acumaticaPayoutPushWorker.EndSession();
        }

    }
}

