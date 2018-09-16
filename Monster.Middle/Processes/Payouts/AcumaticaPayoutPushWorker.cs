using System;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Cash;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Config;
using Monster.Middle.EF;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Payout;

namespace Monster.Middle.Processes.Payouts
{
    public class AcumaticaPayoutPushWorker
    {
        private readonly AcumaticaApiFactory _factory;
        private readonly IPushLogger _logger;
        private readonly PayoutImportRepository _persistRepository;

        public AcumaticaPayoutPushWorker(
                AcumaticaApiFactory factory, 
                IPushLogger logger, 
                PayoutImportRepository persistRepository)
        {
            _factory = factory;
            _logger = logger;
            _persistRepository = persistRepository;
        }

        public void WritePayoutHeaderToAcumatica(
                    AcumaticaCredentials credentials, 
                    long shopifyPayoutId)
        {
            var payout = _persistRepository.RetrievePayout(shopifyPayoutId);

            if (!payout.AcumaticaHeaderId.IsNullOrEmpty())
            {
                _logger.Info(
                    $"Shopify Payout : {payout.ShopifyPayoutId} " + 
                    $"already exists in Acumatica with Id: {payout.AcumaticaHeaderId}");
                return;
            }

            var payoutObject = payout.Json.DeserializeFromJson<Payout>();

            // Notice - this is using the machine's local timezone
            var offset 
                = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);

            var payoutDate 
                = new DateTimeOffset(
                        payoutObject.date.Year,
                        payoutObject.date.Month,
                        payoutObject.date.Day, 
                        0, 0, 0, offset);

            var preferences = _persistRepository.RetrievePayoutPreferences();

            var transaction = new ImportBankTransaction
            {
                CashAccount = preferences.AcumaticaCashAccount.ToValue(),
                StatementDate = payoutDate.ToValue(),
                StartBalanceDate = payoutDate.ToValue(),
                EndBalanceDate = payoutDate.ToValue(),

                //ExtTranID = "JONES-1111-3333".ToValue(),
                //ExtRefNbr = "JONES-1111-3333".ToValue(),
                //Receipt = (100.0).ToValue(),
                //InvoiceNbr = "#5237710".ToValue(),
            };

            // Write the record to Acumatica
            var sessionRepository = 
                _factory.MakeSessionRepository(credentials);
            sessionRepository.RetrieveSession(credentials);

            var repository = 
                _factory.MakeBankRepository(credentials);
            var results = repository.InsertImportBankTransaction(transaction.SerializeToJson());
            _logger.Info(results);

            var acumaticaHeader
                = results.DeserializeFromJson<ImportBankTransaction>();

            _persistRepository
                .UpdatePayoutHeaderAcumaticaImport(
                    shopifyPayoutId,
                    acumaticaHeader.id,
                    acumaticaHeader.ReferenceNbr.value,
                    DateTime.UtcNow);
        }

        public void WritePayoutTransactionsToAcumatica(
                    AcumaticaCredentials credentials,
                    long shopifyPayoutId,
                    long shopifyTransactionId)
        {
            var payout = _persistRepository.RetrievePayout(shopifyPayoutId);
            if (payout.AcumaticaHeaderId.IsNullOrEmpty())
            {
                _logger.Error(
                    "Attempt to write Payout Transactions without Header record " +
                    $"for Shopify Payout {payout.ShopifyPayoutId}");
                return;
            }

            var transactions = _persistRepository.RetrievePayoutTranscations(shopifyPayoutId);

            foreach (var transaction in transactions)
            {
                if (!transaction.AcumaticaRecordId.IsNullOrEmpty())
                {
                    _logger.Info(
                        $"Transaction Payout Id: {transaction.ShopifyPayoutId} - " +
                        $"Transaction Id: {transaction.ShopifyPayoutTransId} " +
                        $"is already written to Acumatica");
                }
                
                var acumaticaTransaction = new ImportBankTransaction
                {
                    id =
                        ExtTranID = "JONES-1111-3333".ToValue(),
                    ExtRefNbr = "JONES-1111-3333".ToValue(),
                    Receipt = (100.0).ToValue(),
                    InvoiceNbr = "#5237710".ToValue(),
                };
            }
        }
    }
}
