﻿using System;
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

        public void GetBankTransactions(AcumaticaCredentials credentials)
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

            if (!payout.AcumaticaHeaderId.IsNullOrEmpty())
            {
                _logger.Info(
                    $"Shopify Payout : {payout.ShopifyPayoutId} " + 
                    $"already exists in Acumatica with Id: {payout.AcumaticaHeaderId}");
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
                    acumaticaHeader.id,
                    acumaticaHeader.ReferenceNbr.value,
                    DateTime.UtcNow);
        }

        public void WritePayoutTransactionsToAcumatica(
                    AcumaticaCredentials credentials,
                    long shopifyPayoutId)
        {
            var payout = _persistRepository.RetrievePayout(shopifyPayoutId);
            var payoutObject = payout.Json.DeserializeFromJson<Payout>();

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

                var transObject = transaction.Json.DeserializeFromJson<PayoutTransaction>();

                if (transObject.type == "payout")
                {
                    _logger.Info(
                        $"Skipping Transaction Payout Id: {transaction.ShopifyPayoutId} - " +
                        $"Transaction Id: {transaction.ShopifyPayoutTransId} " +
                        $"- type = payout");
                    continue;
                }

                var acumaticaTransaction = MakeTransactionHeader(payoutObject);
                
                var receipt = transObject.amount > 0 ? transObject.amount : 0;
                var disbursment = transObject.amount < 0 ? transObject.amount : 0;
                
                acumaticaTransaction.ReferenceNbr = payout.AcumaticaRefNumber.ToValue();
                acumaticaTransaction.ExtTranID 
                    = $"Shopify Order Trans Id: {transObject.source_order_transaction_id}".ToValue();

                acumaticaTransaction.ExtRefNbr
                    = $"Shopify Payout Trans Id: {transObject.id}".ToValue();

                acumaticaTransaction.Receipt = receipt.ToValue();
                acumaticaTransaction.Disbursement = disbursment.ToValue();
                acumaticaTransaction.InvoiceNbr
                    = $"Shopify Order Id: {transObject.source_order_id}".ToValue();

                var repository = _factory.MakeBankRepository(credentials);
                var result = repository.InsertImportBankTransaction(acumaticaTransaction.SerializeToJson());

                var importObject = result.DeserializeFromJson<ImportBankTransaction>();

                _logger.Info(
                    $"Saved Shopify Payout Id: {transaction.ShopifyPayoutId} - " +
                    $"Transaction Id: {transaction.ShopifyPayoutTransId} - " +
                    $"Type: {transObject.type} - " +
                    $"to Acumatica");

                _persistRepository
                    .UpdatePayoutTransactionAcumaticaRecord(
                        transaction.ShopifyPayoutId,
                        transaction.ShopifyPayoutTransId,
                        importObject.id, 
                        DateTime.UtcNow);
            }
        }
    }
}
