using Monster.Acumatica.Http;
using Monster.Middle.Config;
using Monster.Middle.Persist.Multitenant;


namespace Monster.Middle.Processes.Payouts
{
    public class PayoutProcess
    {
        private readonly PayoutConfig _payoutConfig;
        private readonly ShopifyPayoutPullWorker _shopifyPayoutPullWorker;
        private readonly BankImportService _bankImportService;
        private readonly PayoutPersistRepository _persistenceRepository;

        public PayoutProcess(
                PayoutConfig payoutConfig,
                ShopifyPayoutPullWorker shopifyPayoutPullWorker,
                BankImportService bankImportService, 
                PayoutPersistRepository persistenceRepository)
        {
            _payoutConfig = payoutConfig;
            _shopifyPayoutPullWorker = shopifyPayoutPullWorker;
            _bankImportService = bankImportService;
            _persistenceRepository = persistenceRepository;
        }

        public void BeginSession(AcumaticaCredentials credentials)
        {
            _bankImportService.BeginSession(credentials);
        }

        public void EndSession()
        {
            _bankImportService.EndSession();
        }

        public void PullShopifyPayouts(
                        long? shopifyPayoutId = null,
                        bool includeTransactions = true)
        {
            _shopifyPayoutPullWorker.ImportPayoutHeaders(shopifyPayoutId);

            if (includeTransactions)
            {
                if (shopifyPayoutId.HasValue)
                {
                    _shopifyPayoutPullWorker
                        .ImportPayoutTransactions(shopifyPayoutId.Value);
                }
                else
                {
                    _shopifyPayoutPullWorker.ImportIncompletePayoutTransactions();
                }
            }

            // 
            // Entirely optional self-analysis
            //
            //_shopifyPayoutPullWorker
            //    .LogBalancingSummaries(5);
        }

        public void PushAllAcumaticaPayouts()
        {
            foreach (var payout in
                _persistenceRepository.RetrieveNotYetUploadedPayouts())
            {
                _bankImportService
                    .WritePayoutHeaderToAcumatica(payout.ShopifyPayoutId);
            }
        }

        public void PushAcumaticaPayout(long shopifyPayoutId)
        {
            _bankImportService.WritePayoutHeaderToAcumatica(shopifyPayoutId);
        }
    }
}

