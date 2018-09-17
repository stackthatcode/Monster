using System;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Cash;
using Monster.Acumatica.Config;
using Monster.Acumatica.Soap;
using Monster.Acumatica.TestSoap;
using Monster.Middle.EF;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Payout;

namespace Monster.Middle.Processes.Payouts
{
    public class AcumaticaPayoutPushWorkerSoap
    {
        private readonly AcumaticaApiFactory _factory;
        private readonly IPushLogger _logger;
        private readonly PayoutImportRepository _persistRepository;

        public AcumaticaPayoutPushWorkerSoap(
                AcumaticaApiFactory factory, 
                IPushLogger logger, 
                PayoutImportRepository persistRepository)
        {
            _factory = factory;
            _logger = logger;
            _persistRepository = persistRepository;
        }
        

        public ImportBankTransactions MakeTransactionHeader(Payout payoutObject)
        {
            var preferences = _persistRepository.RetrievePayoutPreferences();

            var payoutDate
                = new DateTime(
                    payoutObject.date.Year,
                    payoutObject.date.Month,
                    payoutObject.date.Day);

            var header = new ImportBankTransactions()            
            {
                CashAccount = new StringValue { Value = preferences.AcumaticaCashAccount},                
                StatementDate = new DateTimeValue { Value = payoutDate },
                StartBalanceDate = new DateTimeValue { Value = payoutDate },
                EndBalanceDate = new DateTimeValue { Value = payoutDate },
            };

            return header;
        }


        public void Login(DefaultSoapClient client, AcumaticaCredentials credentials)
        {
            client.Login(
                credentials.Username, 
                credentials.Password, 
                credentials.CompanyName,
                credentials.Branch, 
                null);
        }

        public void WritePayoutHeaderToAcumatica(
                    DefaultSoapClient client,
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
            var results = client.Put(transaction);

            _logger.Info(results.SerializeToJson());
            
            _persistRepository
                .UpdatePayoutHeaderAcumaticaImport(
                    shopifyPayoutId,
                    results.ID.ToString(),
                    null,
                    DateTime.UtcNow);
        }


        public void WritePayoutTransactionsToAcumatica(
                        DefaultSoapClient client,
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

                var acumaticaTrans
                    = MakeTransactionHeader(payoutObject);

                var receipt = transObject.amount > 0 ? transObject.amount : 0;
                var disbursment = transObject.amount < 0 ? transObject.amount : 0;

                acumaticaTrans.ReferenceNbr = "000046".ToSoapValue();

                acumaticaTrans.ExtTranID
                    = $"Shopify Order Trans Id: {transObject.source_order_transaction_id}".ToSoapValue();

                acumaticaTrans.ExtRefNbr
                    = $"Shopify Payout Trans Id: {transObject.id}".ToSoapValue();

                acumaticaTrans.TranDesc = "Test".ToSoapValue();
                acumaticaTrans.RowNumber = new LongValue {Value = 2};
                acumaticaTrans.TranID = (-1).ToSoapValue();

                //acumaticaTrans.LineNbr = 3.ToSoapValue();
                acumaticaTrans.Receipt = new DecimalValue {Value = (decimal)receipt};
                acumaticaTrans.Disbursement = new DecimalValue { Value = (decimal)disbursment };

                acumaticaTrans.InvoiceNbr
                    = $"Shopify Order Id: {transObject.source_order_id}".ToSoapValue();

                var result = client.Put(acumaticaTrans);

                _logger.Info(result.SerializeToJson());

                _logger.Info(
                    $"Saved Shopify Payout Id: {transaction.ShopifyPayoutId} - " +
                    $"Transaction Id: {transaction.ShopifyPayoutTransId} - " +
                    $"Type: {transObject.type} - " +
                    $"to Acumatica");

                _persistRepository
                    .UpdatePayoutTransactionAcumaticaRecord(
                        transaction.ShopifyPayoutId,
                        transaction.ShopifyPayoutTransId,
                        result.ID.ToString(),
                        DateTime.UtcNow);
            }
        }
    }
}
