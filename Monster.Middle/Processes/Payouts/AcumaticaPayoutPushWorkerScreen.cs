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
                    .AddCommand(schema.Actions.Insert)
                    .AddCommand(schema.CashAccount.StartBalanceDate, payoutDate)
                    .AddCommand(schema.CashAccount.EndBalanceDate, payoutDate)
                    .AddCommand(schema.CashAccount.StatementDate, payoutDate);
                    
            commands.AddRange(
                PayoutTransactionsCommands(client, shopifyPayoutId));

            commands
                .AddCommand(schema.Actions.Save)
                .AddCommand(schema.CashAccount.ReferenceNbr);
                
            var results = client.CA306500Submit(commands.ToArray())[0];

            var referenceNbr = results.CashAccount.ReferenceNbr.Value;

            _persistRepository
                .UpdatePayoutHeaderAcumaticaImport(
                        payout.ShopifyPayoutId, referenceNbr, DateTime.UtcNow);
        }


        public List<Command> 
                PayoutTransactionsCommands(
                        Screen client, long shopifyPayoutId)
        {
            var transactions = _persistRepository.RetrievePayoutTranscations(shopifyPayoutId);
            var output = new List<Command>();

            foreach (var transaction in transactions)
            {
                output.AddRange(PayoutTransactionCommands(client, transaction));
            }

            return output;
        }

        public List<Command> PayoutTransactionCommands(
                        Screen client, UsrShopifyPayoutTransaction transaction)
        {
            var transObject = transaction.Json.DeserializeFromJson<PayoutTransaction>();

            if (transObject.type == "payout")
            {
                _logger.Info(
                    $"Skipping Transaction Payout Id: {transaction.ShopifyPayoutId} - " +
                    $"Transaction Id: {transaction.ShopifyPayoutTransId} " +
                    $"- type = payout");
                return new List<Command>();
            }
            
            var receipt = transObject.amount > 0 ? transObject.amount : 0;
            var disbursment = transObject.amount < 0 ? transObject.amount : 0;

            var extTranID = $"Shopify Trans Id: {transObject.source_order_transaction_id}";
            var extRefNbr = $"Shopify Payout Trans Id: {transObject.id}";
            var tranDesc = $"Shopify {transObject.payout_id}";
            var invoiceNbr = $"Shopify Order Id: {transObject.source_order_id}";

            var schema = PX.Soap.Helper.GetSchema<CA306500Content>(client);

            return
                new List<Command>()
                    .AddCommand(schema.Details.ServiceCommands.NewRow)
                    .AddCommand(schema.Details.ExtTranID, extTranID)
                    .AddCommand(schema.Details.ExtRefNbr, extRefNbr)
                    .AddCommand(schema.Details.TranDesc, tranDesc)
                    .AddCommand(schema.Details.InvoiceNbr, invoiceNbr)
                    .AddCommand(schema.Details.Receipt, receipt.ToString())
                    .AddCommand(schema.Details.Disbursement, disbursment.ToString())
                    .AddCommand(schema.Actions.Save)
                    .AddCommand(schema.Details.ExtRefNbr, extRefNbr);            
        }
    }
}
