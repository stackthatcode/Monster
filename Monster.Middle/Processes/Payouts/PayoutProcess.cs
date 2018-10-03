using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;


namespace Monster.Middle.Processes.Payouts
{
    public class PayoutProcess
    {
        private readonly ShopifyPayoutPullWorker _shopifyPayoutPullWorker;
        private readonly BankImportService _bankImportService;
        private readonly PayoutRepository _persistenceRepository;

        public const int DummyCompanyId = 1;


        public PayoutProcess(
                ShopifyPayoutPullWorker shopifyPayoutPullWorker,
                BankImportService bankImportService, 
                PayoutRepository persistenceRepository)
        {
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

        public void PullShopifyPayouts(
                int numberOfHeaders = 1, bool includeTransactions = true)
        {
            _shopifyPayoutPullWorker.ImportPayoutHeaders(numberOfHeaders);

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
        

        public void PushAllAcumaticaPayouts()
        {
            _bankImportService.BeginSession();

            foreach (var payout in
                _persistenceRepository.RetrieveNotYetUploadedPayouts())
            {
                _bankImportService
                    .WritePayoutHeaderToAcumatica(payout.ShopifyPayoutId);
            }

            _bankImportService.EndSession();
        }

        public void PushAcumaticaPayout(long shopifyPayoutId)
        {
            _bankImportService.BeginSession();
            _bankImportService.WritePayoutHeaderToAcumatica(shopifyPayoutId);
            _bankImportService.EndSession();
        }
    }
}

