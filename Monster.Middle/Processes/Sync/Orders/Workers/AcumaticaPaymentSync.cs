using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Payment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Orders.Model;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Sync.Orders.Workers
{
    public class AcumaticaPaymentSync
    {
        private readonly ExecutionLogRepository _logRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly PaymentClient _paymentClient;
        private readonly PreferencesRepository _preferencesRepository;

        public AcumaticaPaymentSync(
                SyncOrderRepository syncOrderRepository, 
                PaymentClient paymentClient, 
                PreferencesRepository preferencesRepository, 
                ExecutionLogRepository logRepository)
        {
            _syncOrderRepository = syncOrderRepository;
            _paymentClient = paymentClient;
            _preferencesRepository = preferencesRepository;
            _logRepository = logRepository;
        }

        public void Run()
        {
            var transactions = _syncOrderRepository.RetrieveUnsyncedTransactions();

            foreach (var transaction in transactions)
            {
                WritePayment(transaction);
            }
        }

        public void WriteUnsyncedPayments(IEnumerable<UsrShopifyTransaction> transactions)
        {
            foreach (var transaction in transactions.Where(x => x.ShouldCreatePayment()))
            {
                if (transaction.ShouldCreatePayment())
                {
                    WritePayment(transaction);
                }
            }
        }

        public void WritePayment(UsrShopifyTransaction transactionRecord)
        {
            // Extract the Transaction Amount
            var transaction
                = transactionRecord
                    .ShopifyJson
                    .DeserializeFromJson<Transaction>();
            var paymentAmount = transaction.amount;
            

            // Arrange data from local cache
            var preferences = _preferencesRepository.RetrievePreferences();
            var paymentMethod = preferences.AcumaticaPaymentMethod;

            var shopifyCustomerId
                = transactionRecord
                    .UsrShopifyOrder.UsrShopifyCustomer.ShopifyCustomerId;

            var customer = _syncOrderRepository.RetrieveCustomer(shopifyCustomerId);
            var acumaticaCustomerId = customer.Match().AcumaticaCustomerId;

            // Build the Payment Ref and Description
            var order = _syncOrderRepository.RetrieveShopifyOrder(transactionRecord.UsrShopifyOrder.ShopifyOrderId);
            var acumaticaOrderRef = order.MatchingSalesOrder().AcumaticaOrderNbr;
            var paymentRef = $"{acumaticaOrderRef}".ToValue();
            var description = $"Created payment for Shopify Order #{order.ShopifyOrderNumber}";

            // Create the payload for Acumatica
            var payment = new PaymentWrite();
            payment.CustomerID = acumaticaCustomerId.ToValue();
            payment.Type = AcumaticaConstants.PaymentType.ToValue();
            payment.PaymentMethod = paymentMethod.ToValue();
            payment.CashAccount = preferences.AcumaticaPaymentCashAccount.ToValue();
            payment.PaymentRef = paymentRef;
            payment.Description = description.ToValue();
            payment.PaymentAmount = ((double)paymentAmount).ToValue();
            payment.OrdersToApply = PaymentOrdersRef.ForOrder(acumaticaOrderRef);

            // Push to Acumatica
            var resultJson = _paymentClient.WritePayment(payment.SerializeToJson());
            var resultPayment = resultJson.DeserializeFromJson<PaymentWrite>();
            var paymentNbr = resultPayment.ReferenceNbr.value;

            // Create Monster Sync Record
            var paymentRecord = new UsrShopifyAcuPayment();
            paymentRecord.UsrShopifyTransaction = transactionRecord;
            paymentRecord.ShopifyPaymentNbr = paymentNbr;
            paymentRecord.DateCreated = DateTime.UtcNow;
            paymentRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.InsertPayment(paymentRecord);

            var log = $"Created Payment {paymentNbr} in Acumatica " +
                      $"from Transaction {transactionRecord.Id} " +
                      $"(Shopify Order #{order.ShopifyOrderNumber})";
            _logRepository.InsertExecutionLog(log);
        }
    }
}
