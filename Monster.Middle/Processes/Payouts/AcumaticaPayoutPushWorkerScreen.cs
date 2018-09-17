using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Monster.Acumatica.Api.Cash;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.BankImportApi;
using Monster.Acumatica.Config;
using Monster.Acumatica.ScreenApi;
using Monster.Middle.EF;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Payout;

namespace Monster.Middle.Processes.Payouts
{
    public class AcumaticaPayoutPushWorkerScreen
    {
        private readonly IPushLogger _logger;
        private readonly PayoutImportRepository _persistRepository;

        public AcumaticaPayoutPushWorkerScreen(
                IPushLogger logger,
                PayoutImportRepository persistRepository)
        {
            _logger = logger;
            _persistRepository = persistRepository;
        }

        public void BeginSession(
                        Screen client, 
                        string screenWebSerivceUrl,
                        AcumaticaCredentials credentials)
        {
            client.CookieContainer = new System.Net.CookieContainer();
            client.Url = screenWebSerivceUrl;
            client.Login(credentials.Username, credentials.Password);
        }

        public void WritePayoutToAcumatica(Screen client, long shopifyPayoutId)
        {
            var payout = _persistRepository.RetrievePayout(shopifyPayoutId);

            if (!payout.AcumaticaRefNumber.IsNullOrEmpty())
            {
                _logger.Info(
                    $"Shopify Payout : {payout.ShopifyPayoutId} " +
                    $"already exists in Acumatica with Ref Number: {payout.AcumaticaRefNumber}");
                return;
            }
            
            // Get data staged
            var payoutObject = payout.Json.DeserializeFromJson<Payout>();
            
            var preferences = _persistRepository.RetrievePayoutPreferences();
            var payoutDate
                = new DateTime(
                        payoutObject.date.Year,
                        payoutObject.date.Month,
                        payoutObject.date.Day)
                    .ToShortDateString();
            
            var schema = PX.Soap.Helper.GetSchema<CA306500Content>(client);
            
            var commands
                = new List<Command>()
                    .AddCommand(schema.CashAccount.CashAccount, preferences.AcumaticaCashAccount)
                    .AddCommand(schema.CashAccount.StartBalanceDate, payoutDate)
                    .AddCommand(schema.CashAccount.EndBalanceDate, payoutDate)
                    .AddCommand(schema.CashAccount.StatementDate, payoutDate)
                    .AddCommand(schema.Actions.Save)
                    .AddCommand(schema.CashAccount.ReferenceNbr);

            var results = client.CA306500Submit(commands.ToArray())[0];

            var referenceNbr = results.CashAccount.ReferenceNbr.Value;

            _persistRepository
                .UpdatePayoutHeaderAcumaticaImport(
                        payout.ShopifyPayoutId, referenceNbr, DateTime.UtcNow);

            WritePayoutTransactionsToAcumatica(client, shopifyPayoutId);
        }


        public void WritePayoutTransactionsToAcumatica(
                        Screen client, long shopifyPayoutId)
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
                WritePayoutTransactionToAcumatica(client, transaction);
            }
        }

        public void WritePayoutTransactionToAcumatica(
                        Screen client, UsrShopifyPayoutTransaction transaction)
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
                return;
            }
            
            var receipt = transObject.amount > 0 ? transObject.amount : 0;
            var disbursment = transObject.amount < 0 ? transObject.amount : 0;

            var extTranID = $"Shopify Trans Id: {transObject.source_order_transaction_id}";
            var extRefNbr = $"Shopify Payout Trans Id: {transObject.id}";
            var tranDesc = $"Shopify {transObject.payout_id}";
            var invoiceNbr = $"Shopify Order Id: {transObject.source_order_id}";

            var schema = PX.Soap.Helper.GetSchema<CA306500Content>(client);

            var commands
                = new List<Command>()
                    .AddCommand(schema.Details.ServiceCommands.NewRow)
                    .AddCommand(schema.Details.ExtTranID, extTranID)
                    .AddCommand(schema.Details.ExtRefNbr, extRefNbr)
                    .AddCommand(schema.Details.TranDesc, tranDesc)
                    .AddCommand(schema.Details.InvoiceNbr, invoiceNbr)
                    .AddCommand(schema.Details.Receipt, receipt.ToString())
                    .AddCommand(schema.Details.Disbursement, disbursment.ToString())
                    .AddCommand(schema.Actions.Save)
                    .AddCommand(schema.CashAccount.ReferenceNbr);

            var results = client.CA306500Submit(commands.ToArray())[0];
            
            _logger.Info(
                $"Saved Shopify Payout Id: {transaction.ShopifyPayoutId} - " +
                $"Transaction Id: {transaction.ShopifyPayoutTransId} - " +
                $"Type: {transObject.type} - " +
                $"to Acumatica");

            //_persistRepository
            //    .UpdatePayoutTransactionAcumaticaRecord(
            //        transaction.ShopifyPayoutId,
            //        transaction.ShopifyPayoutTransId,
            //        importObject.id,
            //        DateTime.UtcNow);

        }
    }
}
