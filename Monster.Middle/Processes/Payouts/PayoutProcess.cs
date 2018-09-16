using Monster.Acumatica.Config;
using Push.Shopify.Http.Credentials;

namespace Monster.Middle.Processes.Payouts
{
    public class PayoutProcess
    {
        private readonly ShopifyPayoutPullWorker _shopifyPayoutPullWorker;
        private readonly AcumaticaPayoutPushWorker _acumaticaPayoutPushWorker;

        public PayoutProcess(
                ShopifyPayoutPullWorker shopifyPayoutPullWorker, 
                AcumaticaPayoutPushWorker acumaticaPayoutPushWorker)
        {
            _shopifyPayoutPullWorker = shopifyPayoutPullWorker;
            _acumaticaPayoutPushWorker = acumaticaPayoutPushWorker;
        }

        public void Execute(
                IShopifyCredentials shopifyCredentials,
                AcumaticaCredentials acumaticaCredentials)
        {
            //_shopifyPayoutPullWorker
            //    .ImportPayoutHeaders(shopifyCredentials, maxPages: 1, recordsPerPage: 10);
            //_shopifyPayoutPullWorker
            //    .ImportIncompletePayoutTransactions(shopifyCredentials);
            //_shopifyPayoutPullWorker
            //    .GenerateBalancingSummaries(10);


            _acumaticaPayoutPushWorker
                .WritePayoutHeaderToAcumatica(
                    acumaticaCredentials, 19248185444, "102000");
        }
    }
}
