using Monster.Middle.Misc.External;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Payouts.Workers;
using Monster.Middle.Processes.Shopify.Persist;


namespace Monster.Middle.Processes.Payouts
{
    public class PayoutProcess
    {
        private readonly ExternalServiceRepository _connectionRepository;
        private readonly ShopifyPayoutPullWorker _shopifyPayoutPullWorker;
        private readonly BankImportService _bankImportService;
        private readonly ShopifyPayoutRepository _persistenceRepository;


        public PayoutProcess(
                ExternalServiceRepository connectionRepository,
                ShopifyPayoutPullWorker shopifyPayoutPullWorker,
                BankImportService bankImportService, 
                ShopifyPayoutRepository persistenceRepository)
        {
            _connectionRepository = connectionRepository;
            _shopifyPayoutPullWorker = shopifyPayoutPullWorker;
            _bankImportService = bankImportService;
            _persistenceRepository = persistenceRepository;
        }

        
        public void PullShopifyPayout(
                long shopifyPayoutId, bool includeTransactions = true)
        {
            _shopifyPayoutPullWorker.RunPayoutHeader(shopifyPayoutId);

            if (includeTransactions)
            {
                _shopifyPayoutPullWorker.RunPayoutTransactions(shopifyPayoutId);
            }
        }

        public void PullShopifyPayouts()
        {
            _shopifyPayoutPullWorker.RunPayoutHeaders();
            _shopifyPayoutPullWorker.RunIncompletePayoutTransations();

            // Entirely optional self-analysis
            //
            //_shopifyPayoutPullWorker
            //    .LogBalancingSummaries(5);
        }
        
        
        public void PushAcumaticaPayout(long shopifyPayoutId)
        {
            _bankImportService.BeginSession();
            _bankImportService.WritePayoutHeaderToAcumatica(shopifyPayoutId);
            _bankImportService.EndSession();
        }
    }
}

