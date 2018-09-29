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



        public void PullShopifyPayout(
                long shopifyPayoutId, bool includeTransactions = true)
        {
            _shopifyPayoutPullWorker.ImportPayoutHeader(shopifyPayoutId);

            if (includeTransactions)
            {
                _shopifyPayoutPullWorker.ImportPayoutTransactions(shopifyPayoutId);
            }
        }

        public void PullShopifyPayouts(bool includeTransactions = true)
        {
            _shopifyPayoutPullWorker.ImportPayoutHeaders();

            if (includeTransactions)
            {
                _shopifyPayoutPullWorker.ImportIncompletePayoutTransactions();
            }

            // 
            // Entirely optional self-analysis
            //
            //_shopifyPayoutPullWorker
            //    .LogBalancingSummaries(5);
        }


        public void BeginAcumaticaSession()
        {
            _bankImportService.BeginSession();
        }

        public void EndAcumaticaSession()
        {
            _bankImportService.EndSession();
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

