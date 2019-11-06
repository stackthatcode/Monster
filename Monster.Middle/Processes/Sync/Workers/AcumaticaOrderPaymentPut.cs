using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Payment;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Sync.Workers
{
    public class AcumaticaOrderPaymentPut
    {
        private readonly ExecutionLogService _logService;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly PaymentClient _paymentClient;
        private readonly SettingsRepository _settingsRepository;

        public AcumaticaOrderPaymentPut(
                    SyncOrderRepository syncOrderRepository, 
                    PaymentClient paymentClient, 
                    SettingsRepository settingsRepository, 
                    ExecutionLogService logService)
        {
            _syncOrderRepository = syncOrderRepository;
            _paymentClient = paymentClient;
            _settingsRepository = settingsRepository;
            _logService = logService;
        }


        public void RunUnsyncedPayments()
        {
            var shopifyOrderIds = _syncOrderRepository.RetrieveOrdersWithUnsyncedTransactions();
            foreach (var shopifyOrderId in shopifyOrderIds)
            {
                RunUnsyncedPayments(shopifyOrderId);
            }
        }

        public void RunUnsyncedPayments(long shopifyOrderId)
        {
            RunPaymentTransaction(shopifyOrderId);
            RunRefundTranscations(shopifyOrderId);
            RunTransactionReleases(shopifyOrderId);
        }


        // Worker methods
        //
        private void RunPaymentTransaction(long shopifyOrderId)
        {
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrderForTransactionSync(shopifyOrderId);
            var transaction = orderRecord.PaymentTransaction();
            var status = PaymentSyncStatus.Make(orderRecord, transaction);

            if (status.ShouldCreatePayment().Success)
            {
                _logService.Log(LogBuilder.CreateAcumaticaPayment(transaction));

                var payment = BuildPayment(transaction);
                WritePaymentAndCreateSync(transaction, payment);
            }
        }

        private void RunRefundTranscations(long shopifyOrderId)
        {
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrderForTransactionSync(shopifyOrderId);

            foreach (var transaction in orderRecord.RefundTransactions())
            {
                var status = PaymentSyncStatus.Make(orderRecord, transaction);
                var shouldCreateRefund = status.ShouldCreateRefundPayment();

                if (shouldCreateRefund.Success)
                {
                    _logService.Log(LogBuilder.CreateAcumaticaCustomerRefund(transaction));

                    var payment = BuildCustomerRefund(transaction);
                    WritePaymentAndCreateSync(transaction, payment);
                }
            }
        }

        private void RunTransactionReleases(long shopifyOrderId)
        {
            var order = _syncOrderRepository.RetrieveShopifyOrderForTransactionSync(shopifyOrderId);

            foreach (var transaction in order.ShopifyTransactions)
            {
                var status = PaymentSyncStatus.Make(order, transaction);
                var shouldRelease = status.ShouldRelease();

                if (shouldRelease.Success)
                {
                    _logService.Log(LogBuilder.ReleasingTransaction(transaction));
                    ReleaseAndUpdateSync(transaction);
                }
            }
        }


        private void WritePaymentAndCreateSync(ShopifyTransaction transactionRecord, PaymentWrite payment)
        {
            // Push to Acumatica
            //
            var resultJson = _paymentClient.WritePayment(payment.SerializeToJson());
            var resultPayment = resultJson.DeserializeFromJson<PaymentWrite>();

            // Create Monster Sync Record
            //
            var paymentRecord = new AcumaticaPayment();
            paymentRecord.ShopifyTransaction = transactionRecord;
            paymentRecord.AcumaticaRefNbr = resultPayment.ReferenceNbr.value;
            paymentRecord.AcumaticaDocType = resultPayment.Type.value;
            paymentRecord.AcumaticaAmount = (decimal)resultPayment.PaymentAmount.value;
            paymentRecord.DateCreated = DateTime.UtcNow;
            paymentRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.InsertPayment(paymentRecord);
        }

        private PaymentWrite BuildPayment(ShopifyTransaction transactionRecord)
        {
            var transaction = transactionRecord.ToShopifyObj();
            var gateway = _settingsRepository.RetrievePaymentGatewayByShopifyId(transaction.gateway);

            // Locate the Acumatica Customer
            //
            var shopifyCustomerId = transactionRecord.CustomerId();
            var customer = _syncOrderRepository.RetrieveCustomer(shopifyCustomerId);
            var acumaticaCustId = customer.AcumaticaCustId();

            // Build the Payment Ref and Description
            //
            var order = _syncOrderRepository.RetrieveShopifyOrder(transactionRecord.OrderId());
            var acumaticaOrderRef = order.AcumaticaSalesOrderId();

            // Create the payload for Acumatica
            //
            var payment = new PaymentWrite();
            payment.CustomerID = acumaticaCustId.ToValue();
            payment.Hold = false.ToValue();
            payment.Type = PaymentType.Payment.ToValue();
            payment.PaymentRef = $"{order.ShopifyOrderNumber}".ToValue();
            payment.PaymentAmount = ((double)transaction.amount).ToValue();
            payment.OrdersToApply = PaymentOrdersRef.ForOrder(acumaticaOrderRef, SalesOrderType.SO);

            payment.PaymentMethod = gateway.AcumaticaPaymentMethod.ToValue();
            payment.CashAccount = gateway.AcumaticaCashAccount.ToValue();
            payment.Description = $"Payment for Shopify Order #{order.ShopifyOrderNumber}".ToValue();

            return payment;
        }

        private PaymentWrite BuildCustomerRefund(ShopifyTransaction transactionRecord)
        {
            var transaction = transactionRecord.ShopifyJson.DeserializeFromJson<Transaction>();

            var paymentGateway = _settingsRepository.RetrievePaymentGatewayByShopifyId(transaction.gateway);

            // Locate the Acumatica Customer
            var shopifyCustomerId = transactionRecord.CustomerId();
            var customer = _syncOrderRepository.RetrieveCustomer(shopifyCustomerId);
            var acumaticaCustId = customer.AcumaticaCustId();

            // Build the Payment Ref and Description
            var order = _syncOrderRepository.RetrieveShopifyOrder(transactionRecord.OrderId());

            // Create the payload for Acumatica
            //
            var refundPayment = new PaymentWrite();
            refundPayment.CustomerID = acumaticaCustId.ToValue();
            refundPayment.Hold = false.ToValue();
            refundPayment.Type = PaymentType.CustomerRefund.ToValue();
            refundPayment.PaymentRef = $"{order.ShopifyOrderNumber}".ToValue();
            refundPayment.PaymentAmount = ((double)transaction.amount).ToValue();

            // Reference to the original Payment
            //
            var acumaticaPayment = order.PaymentTransaction().AcumaticaPayment;
            refundPayment.DocumentsToApply 
                = PaymentDocumentsToApply.ForDocument(
                        acumaticaPayment.AcumaticaRefNbr, acumaticaPayment.AcumaticaDocType, (double)transaction.amount);

            // Amounts
            refundPayment.PaymentMethod = paymentGateway.AcumaticaPaymentMethod.ToValue();
            refundPayment.CashAccount = paymentGateway.AcumaticaCashAccount.ToValue();
            refundPayment.Description = $"Refund for Order #{order.ShopifyOrderNumber} (TransId #{transaction.id})".ToValue();

            return refundPayment;
        }


        private void ReleaseAndUpdateSync(ShopifyTransaction transactionRecord)
        {
            var acumaticaPayment = transactionRecord.AcumaticaPayment;
            _paymentClient.ReleasePayment(acumaticaPayment.AcumaticaRefNbr, acumaticaPayment.AcumaticaDocType);
            acumaticaPayment.IsReleased = true;
            _syncOrderRepository.SaveChanges();
        }
    }
}


