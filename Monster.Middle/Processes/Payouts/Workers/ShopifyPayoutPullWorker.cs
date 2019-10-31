using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Misc.External;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Payout;

namespace Monster.Middle.Processes.Payouts.Workers
{
    public class ShopifyPayoutPullWorker
    {
        private readonly ExternalServiceRepository _connectionRepository;
        private readonly ShopifyBatchRepository _shopifyBatchRepository;
        private readonly ShopifyPayoutRepository _persistRepository;
        private readonly SettingsRepository _settingsRepository;
        private readonly PayoutApi _payoutApi;
        private readonly IPushLogger _logger;

        public int PayoutTransactionPagingLimit = 250;

        public ShopifyPayoutPullWorker(
                ExternalServiceRepository connectionRepository,
                ShopifyBatchRepository shopifyBatchRepository,
                ShopifyPayoutRepository persistRepository,
                SettingsRepository settingsRepository,
                PayoutApi payoutApi,
                IPushLogger logger)
        {
            _connectionRepository = connectionRepository;
            _shopifyBatchRepository = shopifyBatchRepository;
            _persistRepository = persistRepository;
            _settingsRepository = settingsRepository;
            _payoutApi = payoutApi;
            _logger = logger;
        }

        // Use this for Routine methods
        public void RunPayoutHeaders()
        {
            var Settingss = _settingsRepository.RetrieveSettingss();
            var batchState = _shopifyBatchRepository.Retrieve();

            var minDate
                = batchState.ShopifyPayoutGetEnd 
                    ?? Settingss.ShopifyOrderCreatedAtUtc.Value;
            
            // First stage is to import Payouts based on Date 
            var firstPayouts =
                _payoutApi
                    .RetrievePayoutsHeaders(minDate, page: 1, limit: 50)
                    .DeserializeFromJson<PayoutList>();

            UpsertPayoutHeaders(firstPayouts.payouts);

            var currentPage = 2;

            while (true)
            {
                var currentPayouts =
                    _payoutApi
                        .RetrievePayoutsHeaders(minDate, page: currentPage, limit: 50)
                        .DeserializeFromJson<PayoutList>();

                if (currentPayouts.payouts.Count == 0)
                {
                    break;
                }

                UpsertPayoutHeaders(firstPayouts.payouts);
            }

            // Next, import Payout Headers for anything that was either
            // ... in status of "schedule" or "in_transit"
            var unprocessedPayouts 
                = _persistRepository.RetrievePayoutsNotYetProcessedByBank();

            foreach (var unprocessedPayout in unprocessedPayouts)
            {
                RunPayoutHeader(unprocessedPayout.ShopifyPayoutId);
            }
        }

        public void RunPayoutHeader(long shopifyPayoutId)
        {
            var payout =
                _payoutApi
                    .RetrievePayoutHeader(shopifyPayoutId)
                    .DeserializeFromJson<PayoutSingle>();

            UpsertPayoutHeader(payout.payout);
        }

        public void UpsertPayoutHeaders(List<Payout> payouts)
        {
            foreach (var payout in payouts)
            {
                UpsertPayoutHeader(payout);
            }
        }
        
        public void UpsertPayoutHeader(Payout payout)
        {
            var existingPayout = _persistRepository.RetrievePayout(payout.id);

            if (existingPayout != null)
            {
                _persistRepository
                    .UpdatePayoutHeader(
                        payout.id, payout.SerializeToJson(), payout.status);

                _logger.Debug($"Shopify Payout Header {payout.id} found - updating status and skipping!");
                return;
            }
            else
            {
                _logger.Debug($"Creating record for Shopify Payout Header {payout.id}");

                var newPayout = new ShopifyPayout()
                {
                    ShopifyPayoutId = payout.id,
                    ShopifyLastStatus = payout.status,
                    Json = payout.SerializeToJson(),
                    CreatedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                    AllTransDownloaded = false,
                };

                _persistRepository.InsertPayoutHeader(newPayout);
            }
        }

        // Use this for Routine methods
        public void RunIncompletePayoutTransations()
        {
            var payouts = _persistRepository.RetrieveIncompletePayoutImports();

            foreach (var payout in payouts)
            {
                RunPayoutTransactions(payout.ShopifyPayoutId);

                _persistRepository
                    .UpdatePayoutHeaderAllTransDownloaded(payout.ShopifyPayoutId, true);
            }
        }
        
        public void RunPayoutTransactions(long payoutId)
        {
            _logger.Info($"Importing Transactions for Shopify Payout: {payoutId}");

            // Read first batch
            var firstBatch = 
                _payoutApi
                    .RetrievePayoutDetail(
                            payout_id: payoutId, 
                            limit: PayoutTransactionPagingLimit)
                    .DeserializeFromJson<PayoutDetail>();

            UpsertPayoutTransactions(firstBatch, payoutId);

            // Identify transaction id
            var lastTranscationId = firstBatch.transactions.Last().id;

            while (true)
            {
                // Grab the next Batch
                var nextBatch = 
                    _payoutApi
                        .RetrievePayoutDetail(
                                since_id: lastTranscationId, 
                                limit: PayoutTransactionPagingLimit)
                        .DeserializeFromJson<PayoutDetail>();

                UpsertPayoutTransactions(nextBatch, payoutId);
                
                // If there are no transactions, then break!
                if (nextBatch.transactions.Count == 0)
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

        public void UpsertPayoutTransactions(
                        PayoutDetail transactions, long shopifyPayoutId)
        {
            var filteredTransactions
                = transactions
                    .transactions
                    .Where(x => x.payout_id == shopifyPayoutId).ToList();

            var payoutHeader = _persistRepository.RetrievePayout(shopifyPayoutId);

            foreach (var transaction in filteredTransactions)
            {
                var existingRecord =
                    _persistRepository.RetrievePayoutTransaction(
                            transaction.payout_id.Value, transaction.id);
                
                if (existingRecord != null)
                {
                    _logger.Debug(
                        $"Transaction already exists for " +
                        $"Payout Id: {transaction.payout_id} - " + 
                        $"(Transaction) Id {transaction.id}");

                    existingRecord.Json = transaction.SerializeToJson();
                    existingRecord.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    _logger.Debug(
                        $"Creating Transcation for " +
                        $"Payout Id: {transaction.payout_id} - " +
                        $"(Transaction) Id {transaction.id}");

                    var newTransaction = new ShopifyPayoutTransaction()
                    {
                        MonsterParentId = payoutHeader.Id,
                        ShopifyPayoutId = transaction.payout_id.Value,
                        ShopifyPayoutTransId = transaction.id,
                        ShopifyOrderId = transaction.source_order_id,
                        Type = transaction.type,
                        Json = transaction.SerializeToJson(),
                        CreatedDate = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                    };

                    _persistRepository.InsertPayoutTransaction(newTransaction);
                }
            }
        }


        public void LogBalancingSummaries(int limitHowFarBack = 10)
        {
            var payouts = _persistRepository.RetrievePayouts(limitHowFarBack);

            foreach (var payout in payouts)
            {
                var transactions
                    = _persistRepository
                        .RetrievePayoutTransactions(payout.ShopifyPayoutId)
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

