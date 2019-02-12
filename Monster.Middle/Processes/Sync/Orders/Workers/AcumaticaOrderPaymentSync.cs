using System;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Payment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Orders.Model;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Sync.Orders.Workers
{
    public class AcumaticaOrderPaymentSync
    {
        private readonly ExecutionLogRepository _logRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly PaymentClient _paymentClient;
        private readonly PreferencesRepository _preferencesRepository;

        public AcumaticaOrderPaymentSync(
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

        public void RunPaymentsForOrders()
        {
            var transactions = _syncOrderRepository.RetrieveUnsyncedPayments();

            foreach (var transaction in transactions)
            {
                if (transaction.ShouldCreatePayment())
                {
                    WritePaymentForOrders(transaction);
                }
            }
        }
        
        public void WritePaymentForOrders(UsrShopifyTransaction transactionRecord)
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var transaction 
                = transactionRecord.ShopifyJson.DeserializeFromJson<Transaction>();

            // Locate the Acumatica Customer
            var shopifyCustomerId = transactionRecord.CustomerId();
            var customer = _syncOrderRepository.RetrieveCustomer(shopifyCustomerId);
            var acumaticaCustId = customer.AcumaticaCustId();

            // Build the Payment Ref and Description
            var order = _syncOrderRepository.RetrieveShopifyOrder(transactionRecord.OrderId());
            var acumaticaOrderRef = order.AcumaticaSalesOrderId();

            // Create the payload for Acumatica
            var payment = new PaymentWrite();
            payment.CustomerID = acumaticaCustId.ToValue();
            payment.Hold = false.ToValue();
            payment.Type = PaymentType.Payment.ToValue();
            payment.PaymentMethod = preferences.AcumaticaPaymentMethod.ToValue();
            payment.CashAccount = preferences.AcumaticaPaymentCashAccount.ToValue();
            payment.PaymentRef = $"{order.ShopifyOrderNumber}".ToValue();
            payment.PaymentAmount = ((double)transaction.amount).ToValue();
            payment.Description = $"Payment for Shopify Order #{order.ShopifyOrderNumber}".ToValue();
            payment.OrdersToApply =
                    PaymentOrdersRef.ForOrder(acumaticaOrderRef, SalesOrderType.SO);

            // Push to Acumatica
            var resultJson = _paymentClient.WritePayment(payment.SerializeToJson());
            var resultPayment = resultJson.DeserializeFromJson<PaymentWrite>();

            // Create Monster Sync Record
            var paymentRecord = new UsrShopifyAcuPayment();
            paymentRecord.UsrShopifyTransaction = transactionRecord;
            paymentRecord.ShopifyPaymentNbr = resultPayment.ReferenceNbr.value;
            paymentRecord.AcumaticaPaymentType = resultPayment.Type.value;
            paymentRecord.DateCreated = DateTime.UtcNow;
            paymentRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.InsertPayment(paymentRecord);

            // Write Execution Log
            _logRepository.InsertExecutionLog($"Created {payment.Description.value}");
        }



        public void RunPaymentsForRefunds()
        {
            var transactions = _syncOrderRepository.RetrieveUnsyncedRefunds();

            foreach (var transaction in transactions)
            {
                var refund = 
                    _syncOrderRepository
                        .RetrieveRefundAndSync(transaction.ShopifyRefundId.Value);

                WriteRefundPayment(transaction);
            }
        }
        
        public void WriteRefundPayment(UsrShopifyTransaction transactionRecord)
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var transaction 
                = transactionRecord.ShopifyJson.DeserializeFromJson<Transaction>();

            // Locate the Acumatica Customer
            var shopifyCustomerId = transactionRecord.CustomerId();
            var customer = _syncOrderRepository.RetrieveCustomer(shopifyCustomerId);
            var acumaticaCustId = customer.AcumaticaCustId();

            // Build the Payment Ref and Description
            var order = _syncOrderRepository.RetrieveShopifyOrder(transactionRecord.OrderId());
            var acumaticaOrderRef = order.AcumaticaSalesOrderId();

            // Create the JSON for Acumatica
            var payment = new PaymentWrite();
            payment.CustomerID = acumaticaCustId.ToValue();
            payment.Hold = false.ToValue();
            payment.Type = PaymentType.CustomerRefund.ToValue();

            // TODO - add the Credit Memo Invoice as a reference for Returns...?

            // TODO - inject the original Payment Method...?
            // 
            payment.PaymentMethod = preferences.AcumaticaPaymentMethod.ToValue();
            payment.CashAccount = preferences.AcumaticaPaymentCashAccount.ToValue();
            payment.PaymentRef = $"{order.ShopifyOrderNumber}".ToValue();
            payment.PaymentAmount = ((double)transaction.amount).ToValue();
            payment.Description =
                $"Refund for Order #{order.ShopifyOrderNumber} (TransId #{transaction.id})".ToValue();

            payment.OrdersToApply =
                PaymentOrdersRef.ForOrder(acumaticaOrderRef, SalesOrderType.CM);

            // Push to Acumatica
            var resultJson = _paymentClient.WritePayment(payment.SerializeToJson());
            var resultPayment = resultJson.DeserializeFromJson<PaymentWrite>();

            // Create Monster Sync Record
            var paymentRecord = new UsrShopifyAcuPayment();
            paymentRecord.UsrShopifyTransaction = transactionRecord;
            paymentRecord.ShopifyPaymentNbr = resultPayment.ReferenceNbr.value;
            paymentRecord.AcumaticaPaymentType = resultPayment.Type.value;
            paymentRecord.DateCreated = DateTime.UtcNow;
            paymentRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.InsertPayment(paymentRecord);

            // Write Execution Log
            _logRepository.InsertExecutionLog($"Created {payment.Description}");
        }
    }
}


