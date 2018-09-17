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
    public class AcumaticaPayoutPushWorkerRest
    {
        private readonly AcumaticaApiFactory _factory;
        private readonly IPushLogger _logger;
        private readonly PayoutImportRepository _persistRepository;

        public AcumaticaPayoutPushWorkerRest(
                AcumaticaApiFactory factory, 
                IPushLogger logger, 
                PayoutImportRepository persistRepository)
        {
            _factory = factory;
            _logger = logger;
            _persistRepository = persistRepository;
        }

        public void BeginSession(AcumaticaCredentials credentials)
        {
            var sessionRepository = _factory.MakeSessionRepository(credentials);
            sessionRepository.RetrieveSession(credentials);
        }

        public ImportBankTransaction MakeTransactionHeader(Payout payoutObject)
        {
            // Notice - this is using the machine's local timezone
            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);

            var preferences = _persistRepository.RetrievePayoutPreferences();

            var payoutDate
                = new DateTimeOffset(
                    payoutObject.date.Year,
                    payoutObject.date.Month,
                    payoutObject.date.Day,
                    0, 0, 0, offset);

            var transaction = new ImportBankTransaction
            {
                CashAccount = preferences.AcumaticaCashAccount.ToValue(),
                StatementDate = payoutDate.ToValue(),
                StartBalanceDate = payoutDate.ToValue(),
                EndBalanceDate = payoutDate.ToValue(),
            };

            return transaction;
        }

        public void ListBankTransactions(AcumaticaCredentials credentials)
        {
            var repository = _factory.MakeBankRepository(credentials);
            var result = repository.RetrieveImportBankTransactions();
            _logger.Info(result);
        }

        public void WritePayoutHeaderToAcumatica(
                    AcumaticaCredentials credentials, 
                    long shopifyPayoutId)
        {
            var payout = _persistRepository.RetrievePayout(shopifyPayoutId);

            if (!payout.AcumaticaRefNumber.IsNullOrEmpty())
            {
                _logger.Info(
                    $"Shopify Payout : {payout.ShopifyPayoutId} " + 
                    $"already exists in Acumatica with RefNbr: {payout.AcumaticaRefNumber}");
                return;
            }

            var payoutObject = payout.Json.DeserializeFromJson<Payout>();

            var transaction = MakeTransactionHeader(payoutObject);

            // Write the record to Acumatica            
            var repository = _factory.MakeBankRepository(credentials);
            var results = repository.InsertImportBankTransaction(transaction.SerializeToJson());

            _logger.Info(results);

            var acumaticaHeader
                = results.DeserializeFromJson<ImportBankTransaction>();

            _persistRepository
                .UpdatePayoutHeaderAcumaticaImport(
                    shopifyPayoutId,
                    acumaticaHeader.ReferenceNbr.value,
                    DateTime.UtcNow);
        }
        

        public void WritePayoutTransactionsToAcumatica(
                        AcumaticaCredentials credentials,
                        long shopifyPayoutId)
        {
            var payout = _persistRepository.RetrievePayout(shopifyPayoutId);
            var payoutObject = payout.Json.DeserializeFromJson<Payout>();

            if (payout.AcumaticaRefNumber.IsNullOrEmpty())
            {
                _logger.Error(
                    "Attempt to write Payout Transactions without Header record " +
                    $"for Shopify Payout {payout.ShopifyPayoutId}");
                return;
            }

            var transactions = _persistRepository.RetrievePayoutTranscations(shopifyPayoutId);

            foreach (var transaction in transactions)
            {
                var transObject = transaction.Json.DeserializeFromJson<PayoutTransaction>();

                if (transObject.type == "payout")
                {
                    _logger.Info(
                        $"Skipping Transaction Payout Id: {transaction.ShopifyPayoutId} - " +
                        $"Transaction Id: {transaction.ShopifyPayoutTransId} " +
                        $"- type = payout");
                    continue;
                }

                var acumaticaTrans 
                    = MakeTransactionHeader(payoutObject);
                
                var receipt = transObject.amount > 0 ? transObject.amount : 0;
                var disbursment = transObject.amount < 0 ? transObject.amount : 0;
                
                acumaticaTrans.ReferenceNbr = payout.AcumaticaRefNumber.ToValue();
                acumaticaTrans.ExtTranID 
                    = $"Shopify Order Trans Id: {transObject.source_order_transaction_id}".ToValue();

                acumaticaTrans.ExtRefNbr
                    = $"Shopify Payout Trans Id: {transObject.id}".ToValue();

                acumaticaTrans.TranDesc = "Test".ToValue();
                //acumaticaTrans.TranID = "42".ToValue();
                acumaticaTrans.LineNbr = 3.ToValue();
                acumaticaTrans.Receipt = receipt.ToValue();
                acumaticaTrans.Disbursement = disbursment.ToValue();
                acumaticaTrans.InvoiceNbr
                    = $"Shopify Order Id: {transObject.source_order_id}".ToValue();

                var repository = _factory.MakeBankRepository(credentials);
                var result = repository.InsertImportBankTransaction(acumaticaTrans.SerializeToJson());

                var importObject = result.DeserializeFromJson<ImportBankTransaction>();
            }
        }
    }
}
