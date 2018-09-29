using System;
using System.Collections.Generic;
using Monster.Acumatica.BankImportApi;
using Monster.Acumatica.Http;
using Monster.Acumatica.ScreenApi;
using Monster.Middle.Config;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Payout;


namespace Monster.Middle.Processes.Payouts
{
    public class BankImportService : IDisposable
    {
        private readonly IPushLogger _logger;
        private readonly PayoutPersistRepository _persistRepository;
        private readonly PayoutConfig _payoutConfig;
        private readonly Screen _screen;

        // NOTE - entirely separate objects from the Acumatica HTTP

        public BankImportService(                
                IPushLogger logger,
                PayoutPersistRepository persistRepository, 
                PayoutConfig payoutConfig,
                Screen screen)
        {
            _logger = logger;
            _persistRepository = persistRepository;
            _payoutConfig = payoutConfig;
            _screen = screen;
        }


        public void BeginSession(AcumaticaCredentials credentials)
        {
            _screen.CookieContainer = new System.Net.CookieContainer();
            _screen.Url = _payoutConfig.ScreenApiUrl;
            _screen.Login(credentials.Username, credentials.Password);
        }

        public void EndSession()
        {
            _screen.Logout();
        }

        public void WritePayoutHeaderToAcumatica(long shopifyPayoutId)
        {
            var persistedPayout = _persistRepository.RetrievePayout(shopifyPayoutId);
            var schema = PX.Soap.Helper.GetSchema<CA306500Content>(_screen);

            if (persistedPayout.AcumaticaRefNumber.IsNullOrEmpty())
            {
                // Get data staged
                var preferences = _persistRepository.RetrievePayoutPreferences();

                var payoutObject = persistedPayout.Json.DeserializeFromJson<Payout>();

                var payoutDate 
                    = new DateTime(
                        payoutObject.date.Year,
                        payoutObject.date.Month,
                        payoutObject.date.Day)
                    .ToShortDateString();

                var commands
                    = new List<Command>()
                        .AddCommand(schema.CashAccount.CashAccount, preferences.AcumaticaCashAccount)
                        .AddCommand(schema.Actions.Insert)
                        .AddCommand(schema.CashAccount.StartBalanceDate, payoutDate)
                        .AddCommand(schema.CashAccount.EndBalanceDate, payoutDate)
                        .AddCommand(schema.CashAccount.StatementDate, payoutDate)
                        .AddCommand(schema.Actions.Save)
                        .AddCommand(schema.CashAccount.CashAccount)
                        .AddCommand(schema.CashAccount.ReferenceNbr);

                var results = _screen.CA306500Submit(commands.ToArray())[0];

                var cashAccount = results.CashAccount.CashAccount.Value;
                var referenceNbr = results.CashAccount.ReferenceNbr.Value;

                _logger.Info(
                    $"Created Shopify Payout : {persistedPayout.ShopifyPayoutId} " +
                    $"in Acumatica with Ref Number: {referenceNbr}");

                _persistRepository
                    .UpdatePayoutHeaderAcumaticaImport(
                        persistedPayout.ShopifyPayoutId,
                        cashAccount,
                        referenceNbr,
                        DateTime.UtcNow);
            }
            else
            {
                _logger.Info(
                    $"Shopify Payout : {persistedPayout.ShopifyPayoutId} " +
                    $"already exists in Acumatica with Ref Number: {persistedPayout.AcumaticaRefNumber}");
            }

            var navigateCommands
                    = new List<Command>()
                        .AddCommand(schema.CashAccount.CashAccount, persistedPayout.AcumaticaCashAccount)
                        .AddCommand(schema.CashAccount.ReferenceNbr, persistedPayout.AcumaticaRefNumber)
                        .AddCommand(schema.Actions.Cancel);

            var resultnavs = _screen.CA306500Submit(navigateCommands.ToArray());

            WritePayoutTransactions(shopifyPayoutId);
        }
        
        public void WritePayoutTransactions(long shopifyPayoutId)
        {
            var transactions = 
                _persistRepository
                    .RetrieveNotYetUploadedPayoutTranscations(shopifyPayoutId);
            
            foreach (var transaction in transactions)
            {
                WritePayoutTransaction(transaction);
            }
        }

        public void WritePayoutTransaction(
                        UsrShopifyPayoutTransaction transaction)
        {
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

            // *** Careful about juggling all of this
            var tranDesc = $"Shopify Payout Id: {transObject.payout_id} - Payout Trans Id: {transObject.id}";
            var invoiceNbr = $"Shopify Order Trans Id: {transObject.source_order_transaction_id}";
            var extTranID = $"Shopify Order Id: {transObject.source_order_id}";
            var extRefNbr = $"Shopify Payout Trans Id: {transObject.id}";

            var schema = PX.Soap.Helper.GetSchema<CA306500Content>(_screen);

            var commands 
                = new List<Command>()
                    .AddCommand(schema.Details.ServiceCommands.NewRow)
                    .AddCommand(schema.Details.ExtTranID, extTranID)
                    .AddCommand(schema.Details.ExtRefNbr, extRefNbr)
                    .AddCommand(schema.Details.TranDesc, tranDesc)
                    .AddCommand(schema.Details.InvoiceNbr, invoiceNbr)
                    .AddCommand(schema.Details.Receipt, receipt.ToString())
                    .AddCommand(schema.Details.Disbursement, disbursment.ToString())
                    .AddCommand(schema.Actions.Save);

            var results = _screen.CA306500Submit(commands.ToArray());

            _persistRepository
                .UpdatePayoutHeaderAcumaticaImport(
                        transaction.ShopifyPayoutId,
                        transaction.ShopifyPayoutTransId,
                        DateTime.UtcNow, 
                        extRefNbr);

            _logger.Info(
                $"Created Transaction Payout Id: {transaction.ShopifyPayoutId} - " +
                $"Transaction Id: {transaction.ShopifyPayoutTransId} " +
                $" in Acumatica");
        }

        public void Dispose()
        {
            _screen?.Dispose();
        }
    }
}
