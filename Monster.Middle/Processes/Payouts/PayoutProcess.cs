using Monster.Acumatica.Config;
using Monster.Middle.Persistence.Multitenant;
using Push.Shopify.Http.Credentials;

namespace Monster.Middle.Processes.Payouts
{
    public class PayoutProcess
    {
        private readonly ShopifyPayoutPullWorker _shopifyPayoutPullWorker;
        private readonly AcumaticaPayoutPushWorkerScreen _acumaticaPayoutPushWorker;
        private readonly PayoutPersistRepository _persistenceRepository;

        public PayoutProcess(
                ShopifyPayoutPullWorker shopifyPayoutPullWorker,
                AcumaticaPayoutPushWorkerScreen acumaticaPayoutPushWorker, 
                PayoutPersistRepository persistenceRepository)
        {
            _shopifyPayoutPullWorker = shopifyPayoutPullWorker;
            _acumaticaPayoutPushWorker = acumaticaPayoutPushWorker;
            _persistenceRepository = persistenceRepository;
        }

        public void PullShopifyPayouts(
                        int recordsPerPage = 14, 
                        int maxPages = 1,
                        bool includeTransactions = true,
                        long? shopifyPayoutId = null)
        {
            _shopifyPayoutPullWorker
                .ImportPayoutHeaders(
                    maxPages, 
                    recordsPerPage,
                    shopifyPayoutId);

            if (includeTransactions)
            {
                if (shopifyPayoutId.HasValue)
                {
                    _shopifyPayoutPullWorker
                        .ImportPayoutTransactions(
                            payoutId:shopifyPayoutId.Value);
                }
                else
                {
                    _shopifyPayoutPullWorker
                        .ImportIncompletePayoutTransactions();
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

