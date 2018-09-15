using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.EF;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Payout;
using Push.Shopify.Http.Credentials;

namespace Monster.Middle.Workers
{
    public class ShopifyPayoutFetcher
    {
        private readonly PayoutImportRepository _persistRepository;
        private readonly ShopifyApiFactory _shopifyApiFactory;
        private readonly IPushLogger _logger;

        public int PayoutPagingLimit = 50;

        public ShopifyPayoutFetcher(
                PayoutImportRepository persistRepository,
                ShopifyApiFactory shopifyApiFactory,
                IPushLogger logger)
        {
            _persistRepository = persistRepository;
            _shopifyApiFactory = shopifyApiFactory;
            _logger = logger;
        }

        //
        // TODO - add option to use cutoff date
        //
        public void ImportPayoutHeaders(
                IShopifyCredentials credentials, 
                int maxPages = 1, int recordsPerPage = 1)
        {
            var payoutApi = _shopifyApiFactory.MakePayoutApi(credentials);
            var currentPage = 1;

            while (currentPage <= maxPages)
            {
                // Get current list of Payouts
                var payouts =
                    payoutApi
                        .RetrievePayouts(recordsPerPage, maxPages)
                        .DeserializeFromJson<PayoutList>();

                SavePayoutHeaders(payouts);

                currentPage++;
            }
        }

        public void SavePayoutHeaders(PayoutList payouts)
        {
            foreach (var payout in payouts.payouts)
            {
                var persistedPayout =
                    _persistRepository.RetrievePayout(payout.id);

                if (persistedPayout != null)
                {
                    _logger.Info($"Shopify Payout {payout.id} found - skipping");
                    continue;
                }

                _logger.Info(
                    $"Shopify Payout {payout.id} not found - saving to persistence");

                var newPayout = new UsrShopifyPayout()
                {
                    ShopifyPayoutId = payout.id,
                    Json = payout.SerializeToJson(),
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    AllDetailRecordsCaptured = false,
                };

                _persistRepository.InsertPayoutHeader(newPayout);
            }
        }
        
        public void ImportIncompletePayoutTransactions(IShopifyCredentials credentials)
        {
            var payouts = _persistRepository.RetrieveIncompletePayoutImports();

            foreach (var payout in payouts)
            {
                ImportPayoutTransactions(credentials, payout.Id);
            }
        }
        
        public void ImportPayoutTransactions(
                        IShopifyCredentials credentials, long payoutId)
        {
            var payoutApi = _shopifyApiFactory.MakePayoutApi(credentials);

            // Read first batch
            var firstBatch = payoutApi
                    .RetrievePayoutDetail(
                            payout_id: payoutId, limit: PayoutPagingLimit)
                    .DeserializeFromJson<PayoutDetail>();

            PersistPayoutTransactions(firstBatch);

            // Identify transaction id
            var lastTranscationId = firstBatch.transactions.Last().id;

            while (true)
            {
                // Grab the next Batch
                var nextBatch = payoutApi
                        .RetrievePayoutDetail(
                                since_id: lastTranscationId, limit: PayoutPagingLimit)
                        .DeserializeFromJson<PayoutDetail>();
                
                // We stop iterating when we see type = "payout"
                if (nextBatch.transactions.Any(x => x.type == "payout"))
                {
                    break;
                }

                // Else, grab the last transaction id and keep going!
                lastTranscationId = nextBatch.transactions.Last().id;
            }
        }

        //// We'll dump our output to JSON for easy review
        //var filteredTransactions
        //    = transactions.Where(x => x.payout_id != null).ToList();

        //// Total up all of the charges
        //var charges = filteredTransactions.Where(x => x.type == "charge").ToList();
        //_logger.Info($"Total Charges -> Amount: {charges.Sum(x => x.amount)}");
        //_logger.Info($"Total Charges -> Fee: {charges.Sum(x => x.fee)}");
        //_logger.Info($"Total Charges -> Net: {charges.Sum(x => x.net)}");
        //
        //var payout = filteredTransactions.First(x => x.type == "payout");
        //_logger.Info($"Payout -> Amount: {payout.amount}");
        //_logger.Info($"Payout -> Fee: {payout.fee}");
        //_logger.Info($"Payout -> Net: {payout.net}");


        public void PersistPayoutTransactions(PayoutDetail transactions)
        {
            var filteredTransactions
                = transactions
                    .transactions
                    .Where(x => x.payout_id != null).ToList();

            foreach (var transaction in filteredTransactions)
            {
                var persistedTransaction =
                    _persistRepository.RetrievePayoutTransaction(
                        transaction.payout_id.Value, transaction.id);

                if (persistedTransaction != null)
                {
                    _logger.Info(
                        $"Transaction already exists for " +
                        $"Payout Id: {transaction.payout_id} - " + 
                        $"(Transaction) Id {transaction.id}");
                    continue;
                }

                var newTransaction = new UsrShopifyPayoutTransaction()
                {
                    ShopifyPayoutId = transaction.payout_id.Value,
                    ShopifyPayoutTransId = transaction.id,
                    Json = transaction.SerializeToJson(),
                    CreatedDate = DateTime.UtcNow,
                };

                _persistRepository.InsertPayoutTransaction(newTransaction);
            }
        }
    }
}

