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
using Monster.Middle.Processes.Sync.Services;
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
        private readonly OrderSyncValidationService _validationService;

        public AcumaticaOrderPaymentPut(
                    SyncOrderRepository syncOrderRepository, 
                    PaymentClient paymentClient, 
                    SettingsRepository settingsRepository, 
                    ExecutionLogService logService, 
                    OrderSyncValidationService validationService)
        {
            _syncOrderRepository = syncOrderRepository;
            _paymentClient = paymentClient;
            _settingsRepository = settingsRepository;
            _logService = logService;
            _validationService = validationService;
        }


        public void RunUnsyncedPayments()
        {
            var unsyncedOrderIds = _syncOrderRepository.RetrieveOrdersWithUnreleasedTransactions();
            foreach (var shopifyOrderId in unsyncedOrderIds)
            {
                RunTransactionReleases(shopifyOrderId);
            }

            var unreleasedOrderIds = _syncOrderRepository.RetrieveOrdersWithUnsyncedTransactions();
            foreach (var shopifyOrderId in unreleasedOrderIds)
            {
                RunUnsyncedPayments(shopifyOrderId);
            }
        }


        public void RunUnsyncedPayments(long shopifyOrderId)
        {
            RunPaymentTransaction(shopifyOrderId);
            RunRefundTranscations(shopifyOrderId);
        }


        // Worker methods
        //
        private void RunPaymentTransaction(long shopifyOrderId)
        {
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrderWithNoTracking(shopifyOrderId);
            var transaction = orderRecord.PaymentTransaction();
            var status = _validationService.GetTransSyncValidator(orderRecord, transaction);

            var createValidation = status.ReadyToCreatePayment();
            if (createValidation.Success)
            {
                // Create new Payment in Acumatica
                //
                _logService.Log(LogBuilder.CreateAcumaticaPayment(transaction));
                var payment = BuildPaymentForCreate(transaction);
                PushToAcumaticaAndWriteRecord(transaction, payment);

                // Upon creation of Payment, run Release process
                //
                RunTransactionReleases(shopifyOrderId);
                return;
            }

            var updateValidation = status.ReadyToUpdatePayment();
            if (updateValidation.Success)
            {
                _logService.Log(LogBuilder.UpdateAcumaticaPayment(transaction));
                var payment = BuildPaymentForUpdate(transaction);
                PushToAcumaticaAndWriteRecord(transaction, payment);
            }
        }

        private void RunRefundTranscations(long shopifyOrderId)
        {
            var transactions = _syncOrderRepository.RetrieveUnsyncedTransactions(shopifyOrderId);

            foreach (var transaction in transactions)
            {
                var orderRecord = _syncOrderRepository.RetrieveShopifyOrderWithNoTracking(shopifyOrderId);

                var status = _validationService.GetTransSyncValidator(orderRecord, transaction);

                var readyToCreateRefundPayment = status.ReadyToCreateRefundPayment();
                
                if (readyToCreateRefundPayment.Success)
                {
                    _logService.Log(LogBuilder.CreateAcumaticaCustomerRefund(transaction));

                    var payment = BuildCustomerRefund(transaction);
                    PushToAcumaticaAndWriteRecord(transaction, payment);
                }

                // Immediately run Transaction Releases afterward
                //
                RunTransactionReleases(shopifyOrderId);
            }
        }

        private void RunTransactionReleases(long shopifyOrderId)
        {
            var transactions = _syncOrderRepository.RetrieveUnsyncedTransactions(shopifyOrderId);

            foreach (var transaction in transactions)
            {
                var order = _syncOrderRepository.RetrieveShopifyOrderWithNoTracking(shopifyOrderId);
                var status = _validationService.GetTransSyncValidator(order, transaction);
                var shouldRelease = status.ReadyToRelease();

                if (shouldRelease.Success)
                {
                    _logService.Log(LogBuilder.ReleasingTransaction(transaction));
                    ReleaseAndUpdateSync(transaction);
                }
            }
        }


        private PaymentWrite BuildPaymentForCreate(ShopifyTransaction transactionRecord)
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

            // Amount computations
            //
            payment.PaymentAmount = ((double)transaction.amount).ToValue();
            var appliedToOrder = order.NetPaymentAppliedToOrder();
            payment.OrdersToApply = PaymentOrdersRef.ForOrder(
                acumaticaOrderRef, SalesOrderType.SO, (double)appliedToOrder);

            payment.PaymentMethod = gateway.AcumaticaPaymentMethod.ToValue();
            payment.CashAccount = gateway.AcumaticaCashAccount.ToValue();
            payment.Description = $"Payment for Shopify Order #{order.ShopifyOrderNumber}".ToValue();

            return payment;
        }

        private PaymentWrite BuildPaymentForUpdate(ShopifyTransaction transactionRecord)
        {
            // Build the Payment Ref and Description
            //
            var order = _syncOrderRepository.RetrieveShopifyOrder(transactionRecord.OrderId());
            var acumaticaOrderRef = order.AcumaticaSalesOrderId();

            // Applied To Order
            var amountApplied = order.NetPaymentAppliedToOrder();

            // Create the payload for Acumatica
            //
            var payment = new PaymentWrite();
            payment.ReferenceNbr = transactionRecord.AcumaticaPayment.AcumaticaRefNbr.ToValue();
            payment.Type = PaymentType.Payment.ToValue();
            payment.OrdersToApply 
                = PaymentOrdersRef.ForOrder(
                        acumaticaOrderRef, SalesOrderType.SO, (double)amountApplied);

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


        private void PushToAcumaticaAndWriteRecord(ShopifyTransaction transactionRecord, PaymentWrite payment)
        {
            // Push to Acumatica
            //
            var resultJson = _paymentClient.WritePayment(payment.SerializeToJson());
            var resultPayment = resultJson.DeserializeFromJson<PaymentWrite>();

            var existingRecord = _syncOrderRepository.RetreivePayment(transactionRecord.Id);

            if (existingRecord == null)
            {
                // Create Monster Sync Record
                //
                var paymentRecord = new AcumaticaPayment();
                paymentRecord.ShopifyTransactionMonsterId = transactionRecord.Id;
                paymentRecord.AcumaticaRefNbr = resultPayment.ReferenceNbr.value;
                paymentRecord.AcumaticaDocType = resultPayment.Type.value;
                paymentRecord.AcumaticaAmount = (decimal) resultPayment.PaymentAmount.value;
                paymentRecord.AcumaticaAppliedToOrder = (decimal) payment.AmountAppliedToOrder;
                paymentRecord.DateCreated = DateTime.UtcNow;
                paymentRecord.LastUpdated = DateTime.UtcNow;
                _syncOrderRepository.InsertPayment(paymentRecord);
            }
            else
            {
                existingRecord.AcumaticaAppliedToOrder = (decimal)payment.AmountAppliedToOrder;
                existingRecord.LastUpdated = DateTime.UtcNow;
                _syncOrderRepository.SaveChanges();
            }

            _syncOrderRepository.UpdateShopifyTransactionNeedsPut(transactionRecord.Id, false);
        }



        private void ReleaseAndUpdateSync(ShopifyTransaction transactionRecord)
        {
            var acumaticaPayment = transactionRecord.AcumaticaPayment;
            _paymentClient.ReleasePayment(acumaticaPayment.AcumaticaRefNbr, acumaticaPayment.AcumaticaDocType);
            _syncOrderRepository.PaymentIsReleased(acumaticaPayment.ShopifyTransactionMonsterId);
        }
    }
}


