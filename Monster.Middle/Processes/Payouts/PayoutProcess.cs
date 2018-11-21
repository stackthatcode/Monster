using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Shopify;
using Monster.Middle.Processes.Orders.Workers;
using Monster.Middle.Processes.Payouts.Workers;


namespace Monster.Middle.Processes.Payouts
{
    public class PayoutProcess
    {
        private readonly TenantRepository _tenantRepository;
        private readonly ShopifyPayoutPullWorker _shopifyPayoutPullWorker;
        private readonly BankImportService _bankImportService;
        private readonly ShopifyPayoutRepository _persistenceRepository;


        public PayoutProcess(
                TenantRepository tenantRepository,
                ShopifyPayoutPullWorker shopifyPayoutPullWorker,
                BankImportService bankImportService, 
                ShopifyPayoutRepository persistenceRepository)
        {
            _tenantRepository = tenantRepository;
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

