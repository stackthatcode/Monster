using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Monster.Acumatica.Api;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http.Cash;
using Monster.Acumatica.Http.Common;
using Monster.Middle.EF;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Payout;

namespace Monster.Middle.Workers
{
    public class AcumaticaPayoutPush
    {
        private readonly AcumaticaApiFactory _factory;
        private readonly IPushLogger _logger;
        private readonly PayoutImportRepository _persistRepository;

        public AcumaticaPayoutPush(
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
                    long shopifyPayoutId,
                    string acumaticaCashAccount)
        {
            var payout = _persistRepository.RetrievePayout(shopifyPayoutId);

            if (!payout.AcumaticaImportId.IsNullOrEmpty())
            {
                _logger.Info(
                    $"Shopify Payout : {payout.ShopifyPayoutId} " + 
                    $"already exists in Acumatica with Id: {payout.AcumaticaImportId}");
                return;
            }

            var payoutObject = payout.Json.DeserializeFromJson<Payout>();

            // Get the Bank Transactions
            // TODO - create simple UTC wrapper
            var payoutDate 
                = new DateTimeOffset(
                        payoutObject.date.Year,
                        payoutObject.date.Month,
                        payoutObject.date.Day, 
                        0, 0, 0, TimeSpan.Zero);

            var transaction = new ImportBankTransaction
            {
                CashAccount = acumaticaCashAccount.ToValue(),
                StatementDate = payoutDate.ToValue(),
                StartBalanceDate = payoutDate.ToValue(),
                EndBalanceDate = payoutDate.ToValue(),

                //ExtTranID = "JONES-1111-3333".ToValue(),
                //ExtRefNbr = "JONES-1111-3333".ToValue(),
                //Receipt = (100.0).ToValue(),
                //InvoiceNbr = "#5237710".ToValue(),
            };


            // Create the repository passing the credentials and create session
            var sessionRepository = _factory.MakeSessionRepository(credentials);
            sessionRepository.RetrieveSession(credentials);
            var repository = _factory.MakeBankRepository(credentials);
            var results = repository.InsertImportBankTransaction(transaction.SerializeToJson());
            _logger.Info(results);

            // TODO - save to SQL
        }
    }
}
