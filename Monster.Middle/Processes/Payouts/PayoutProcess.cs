using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;


namespace Monster.Middle.Processes.Payouts
{
    public class PayoutProcess
    {
        private readonly PersistContext _persistContext;
        private readonly ShopifyHttpContext _shopifyContext;
        private readonly AcumaticaHttpContext _acumaticaHttpContext;
        private readonly PayoutConfig _payoutConfig;
        private readonly ShopifyPayoutPullWorker _shopifyPayoutPullWorker;
        private readonly BankImportService _bankImportService;
        private readonly PayoutPersistRepository _persistenceRepository;

        public const int DummyCompanyId = 1;


        public PayoutProcess(
                PersistContext persistContext,
                ShopifyHttpContext shopifyContext,
                AcumaticaHttpContext acumaticaHttpContext,
                PayoutConfig payoutConfig,

                ShopifyPayoutPullWorker shopifyPayoutPullWorker,
                BankImportService bankImportService, 
                PayoutPersistRepository persistenceRepository)
        {
            _persistContext = persistContext;
            _shopifyContext = shopifyContext;
            _acumaticaHttpContext = acumaticaHttpContext;
            _payoutConfig = payoutConfig;
            _shopifyPayoutPullWorker = shopifyPayoutPullWorker;
            _bankImportService = bankImportService;
            _persistenceRepository = persistenceRepository;
        }


        // This is slightly uncommon - the explicit injection credentials
        public void Initialize(
                    string connectionString,
                    PrivateAppCredentials shopifyCredentials,
                    AcumaticaCredentials acumaticaCredentials,
                    string bankImportScreenUrl)
        {
            _persistContext.Initialize(connectionString, DummyCompanyId);
            _shopifyContext.Initialize(shopifyCredentials);
            _acumaticaHttpContext.Initialize(acumaticaCredentials);
            _payoutConfig.ScreenApiUrl = bankImportScreenUrl;
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

