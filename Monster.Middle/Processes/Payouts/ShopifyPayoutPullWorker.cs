using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Config;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Payout;

namespace Monster.Middle.Processes.Payouts
{
    public class ShopifyPayoutPullWorker
    {
        private readonly PayoutDataRepository _persistRepository;
        private readonly PayoutApi _payoutRepository;
        private readonly PayoutConfig _payoutConfig;
        private readonly IPushLogger _logger;

        public int PayoutTransactionPagingLimit = 250;

        public ShopifyPayoutPullWorker(
                    PayoutDataRepository persistRepository,
                    PayoutApi payoutRepository,
                    IPushLogger logger)
        {
            _persistRepository = persistRepository;
            _payoutRepository = payoutRepository;
            _logger = logger;
        }

        public void ImportPayoutHeader(long shopifyPayoutId)
        {
            var payout =
                _payoutRepository
                    .RetrievePayoutHeader(shopifyPayoutId)
                    .DeserializeFromJson<PayoutSingle>();

            SavePayout(payout.payout);
        }

        //
        // TODO - add option to use cutoff date...?
        //
        public void ImportPayoutHeaders(int numberOfHeadersToImport)
        {
            // Get current list of Payouts
            var payouts =
                _payoutRepository
                    .RetrievePayoutsHeaders(numberOfHeadersToImport)
                    .DeserializeFromJson<PayoutList>();
            
            foreach (var payout in payouts.payouts)
            {
                SavePayout(payout);
            }
        }

        public void SavePayout(Payout payout)
        {
            var persistedPayout = _persistRepository.RetrievePayout(payout.id);

            if (persistedPayout != null)
            {
                _persistRepository
                    .UpdatePayoutHeaderStatus(payout.id, payout.status);

                _logger.Info($"Shopify Payout {payout.id} found - updating status and skipping!");
                return;
            }

            _logger.Info(
                $"Creating Header for Shopify Payout {payout.id}");

            var newPayout = new UsrShopifyPayout()
            {
                ShopifyPayoutId = payout.id,
                ShopifyLastStatus = payout.status,
                Json = payout.SerializeToJson(),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                AllShopifyTransDownloaded = false,
            };

            _persistRepository.InsertPayoutHeader(newPayout);
        }

        public void ImportIncompletePayoutTransactions()
        {
            var payouts = _persistRepository.RetrieveIncompletePayoutImports();

            foreach (var payout in payouts)
            {
                ImportPayoutTransactions(payout.ShopifyPayoutId);

                _persistRepository
                    .UpdatePayoutHeaderAllShopifyTransDownloaded(
                            payout.ShopifyPayoutId, true);
            }
        }
        
        public void ImportPayoutTransactions(long payoutId)
        {
            _logger.Info($"Importing Transactions for Shopify Payout: {payoutId}");

            // Read first batch
            var firstBatch = 
                _payoutRepository
                    .RetrievePayoutDetail(
                            payout_id: payoutId, 
                            limit: PayoutTransactionPagingLimit)
                    .DeserializeFromJson<PayoutDetail>();

            PersistPayoutTransactions(firstBatch, payoutId);

            // Identify transaction id
            var lastTranscationId = firstBatch.transactions.Last().id;

            while (true)
            {
                // Grab the next Batch
                var nextBatch = 
                    _payoutRepository
                        .RetrievePayoutDetail(
                                since_id: lastTranscationId, limit: PayoutTransactionPagingLimit)
                        .DeserializeFromJson<PayoutDetail>();

                PersistPayoutTransactions(nextBatch, payoutId);
                
                // If there are fewer transactions than the paging limit, break!
                if (nextBatch.transactions.Count < PayoutTransactionPagingLimit)
                {
                    break;
                }

                // We stop iterating when we see type = "payout" or a null payout id
                if (nextBatch
                        .transactions
                        .Any(x => x.type == "payout" || 
                                  x.payout_id == null || 
                                  x.payout_id != payoutId))
                {
                    break;
                }

                // Else, grab the last transaction id and keep going!
                lastTranscationId = nextBatch.transactions.Last().id;
            }
        }

        public void PersistPayoutTransactions(
                        PayoutDetail transactions, long shopifyPayoutId)
        {
            var filteredTransactions
                = transactions
                    .transactions
                    .Where(x => x.payout_id == shopifyPayoutId).ToList();

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

                _logger.Info(
                    $"Creating Transcation for " +
                    $"Payout Id: {transaction.payout_id} - " +
                    $"(Transaction) Id {transaction.id}");

                var newTransaction = new UsrShopifyPayoutTransaction()
                {
                    Type = transaction.type,
                    ShopifyPayoutId = transaction.payout_id.Value,
                    ShopifyPayoutTransId = transaction.id,
                    Json = transaction.SerializeToJson(),
                    CreatedDate = DateTime.UtcNow,
                };

                _persistRepository.InsertPayoutTransaction(newTransaction);
            }
        }


        public void LogBalancingSummaries(int limitHowFarBack = 10)
        {
            var payouts = _persistRepository.RetrievePayouts(limitHowFarBack);

            foreach (var payout in payouts)
            {
                var transactions
                    = _persistRepository
                        .RetrieveNotYetUploadedPayoutTranscations(payout.ShopifyPayoutId)
                        .Select(x => x.Json.DeserializeFromJson<PayoutTransaction>())
                        .ToList();

                _logger.Info($"Balancing Payout: {payout.ShopifyPayoutId}");

                TotalHelper(transactions, "charge");
                TotalHelper(transactions, "refund");
                TotalHelper(transactions, "adjustment");
                TotalHelper(transactions, "payout");
                _logger.Info(Environment.NewLine);
            }
        }

        private void TotalHelper(
                    IEnumerable<PayoutTransaction> transactions, string type)
        {
            var transByType = transactions.Where(x => x.type == type).ToList();
            _logger.Info($"Total [type='{type}'] -> Amount: {transByType.Sum(x => x.amount):0.00}");
            _logger.Info($"Total [type='{type}'] -> Fee: {transByType.Sum(x => x.fee):0.00}");
            _logger.Info($"Total [type='{type}'] -> Net: {transByType.Sum(x => x.net):0.00}");

        }

    }
}

