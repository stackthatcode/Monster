using System;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Payment;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Analysis;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.PendingActions;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Sync.Workers
{
    public class AcumaticaOrderPaymentPut
    {
        private readonly ExecutionLogService _logService;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly PaymentClient _paymentClient;
        private readonly SettingsRepository _settingsRepository;
        private readonly PendingActionService _pendingActionService;
        private readonly IPushLogger _systemLogger;

        public AcumaticaOrderPaymentPut(
                    SyncOrderRepository syncOrderRepository, 
                    PaymentClient paymentClient, 
                    SettingsRepository settingsRepository, 
                    ExecutionLogService logService, 
                    PendingActionService pendingActionService, 
                    IPushLogger systemLogger)
        {
            _syncOrderRepository = syncOrderRepository;
            _paymentClient = paymentClient;
            _settingsRepository = settingsRepository;
            _logService = logService;
            _pendingActionService = pendingActionService;
            _systemLogger = systemLogger;
        }


        public void RunAutomatic()
        {
            var processingList = _syncOrderRepository.RetrieveOrdersWithUnsyncedTransactions();
            processingList.AddRange(_syncOrderRepository.RetrieveOrdersWithUnreleasedTransactions());

            var shopifyOrderIds = processingList.Distinct().OrderBy(x => x);
            foreach (var shopifyOrderId in shopifyOrderIds)
            {
                ProcessOrder(shopifyOrderId);
            }
        }

        public bool ProcessOrder(long shopifyOrderId)
        {
            try
            {
                _systemLogger.Debug($"AcumaticaOrderPaymentPut for Shopify Order {shopifyOrderId}");

                // Clear-out any un-Released Transaction
                //
                ProcessAllTransactionReleases(shopifyOrderId);

                // Refresh Status and run for Payment Transaction
                //
                ProcessPaymentTransaction(shopifyOrderId);
                ProcessAllTransactionReleases(shopifyOrderId);

                // Refresh Status and run for Refund Transactions
                //
                var rootAction = _pendingActionService.Create(shopifyOrderId);
                foreach (var refundAction in rootAction.RefundPaymentActions)
                {
                    ProcessRefundTransaction(refundAction);
                    ProcessTransactionRelease(refundAction);
                }

                _syncOrderRepository.UpdateShopifyHasError(shopifyOrderId, false);
                return true;
            }
            catch(Exception ex)
            {
                _systemLogger.Error(ex);
                _logService.Log($"Encounter error syncing Payments for Shopify Order {shopifyOrderId}");

                _syncOrderRepository.UpdateShopifyHasError(shopifyOrderId, true);
                return false;
            }
        }


        private void ProcessPaymentTransaction(long shopifyOrderId)
        {
            var rootAction = _pendingActionService.Create(shopifyOrderId);
            var paymentAction = rootAction.PaymentAction;
                
            if (paymentAction.ActionCode == ActionCode.None)
            {
                return;
            }

            if (paymentAction.ActionCode == ActionCode.CreateInAcumatica && paymentAction.IsValid)
            {
                // Build Create Payment payload for Acumatica
                //
                var transaction =
                    _syncOrderRepository
                        .RetrieveShopifyTransactionWithNoTracking(paymentAction.ShopifyTransactionId);
                var payment = BuildPaymentForCreate(transaction);

                // Push to Acumatica and write Sync Record 
                //
                _logService.Log(LogBuilder.CreateAcumaticaPayment(transaction));
                PushPaymentAndWriteSync(transaction, payment);
                return;
            }

            if (paymentAction.ActionCode == ActionCode.UpdateInAcumatica && paymentAction.IsValid)
            {
                // Build Payment Update payload for Acumatica
                //
                var transaction =
                    _syncOrderRepository
                        .RetrieveShopifyTransactionWithNoTracking(paymentAction.ShopifyTransactionId);
                var payment = BuildPaymentForUpdate(transaction);

                // Push to Acumatica and write Sync Record 
                //
                _logService.Log(LogBuilder.UpdateAcumaticaPayment(transaction));
                PushPaymentAndWriteSync(transaction, payment);
            }
        }

        private void ProcessRefundTransaction(PaymentAction refundAction)
        {
            if (refundAction.ActionCode == ActionCode.None)
            {
                return;
            }

            if (refundAction.ActionCode == ActionCode.CreateInAcumatica && refundAction.IsValid)
            {
                // Create Acumatica Refund payload
                // 
                var transaction =
                    _syncOrderRepository
                        .RetrieveShopifyTransactionWithNoTracking(refundAction.ShopifyTransactionId);

                var refundWrite = BuildCustomerRefund(transaction);

                // Push to Acumatica and write Sync Record
                //
                _logService.Log(LogBuilder.CreateAcumaticaPayment(transaction));
                PushPaymentAndWriteSync(transaction, refundWrite);
            }
        }


        private void ProcessAllTransactionReleases(long shopifyOrderId)
        {
            var status = _pendingActionService.Create(shopifyOrderId);

            ProcessTransactionRelease(status.PaymentAction);

            foreach (var refund in status.RefundPaymentActions)
            {
                ProcessTransactionRelease(refund);
            }
        }

        private void ProcessTransactionRelease(PaymentAction status)
        {
            if (status.ActionCode == ActionCode.ReleaseInAcumatica && status.IsValid)
            {
                var transaction
                    = _syncOrderRepository
                        .RetrieveShopifyTransactionWithNoTracking(status.ShopifyTransactionId);

                _logService.Log(LogBuilder.ReleasingTransaction(transaction));
                PushReleaseAndUpdateSync(transaction);
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
            payment.PaymentRef = $"{transaction.id}".ToValue();

            // Amount computations
            //
            payment.PaymentAmount = ((double)transaction.amount).ToValue();
            var appliedToOrder = order.NetRemainingPayment();

            payment.OrdersToApply = 
                PaymentOrdersRef.ForOrder(acumaticaOrderRef, SalesOrderType.SO, (double)appliedToOrder);

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
            var paymentNbr = transactionRecord.AcumaticaPayment.AcumaticaRefNbr;

            // Applied To Order
            var paymentMinusRefundsAndDebits = order.NetRemainingPayment();

            // Doing this to force Acumatica to load the Payment into the cache
            //
            var paymentInAcumatica = _paymentClient.RetrievePayment(paymentNbr, PaymentType.Payment);

            // *** On hold for now - until Invoices are Released, Acumatica will error on certain API calls
            //var appliedToDocuments = paymentInAcumatica.AppliedToDocuments.value;
            //var appliedToOrder = paymentMinusRefundsAndDebits - (decimal)appliedToDocuments;

            // Create the payload for Acumatica
            //
            var payment = new PaymentWrite();
            payment.ReferenceNbr = paymentNbr.ToValue();
            payment.Type = PaymentType.Payment.ToValue();
            payment.OrdersToApply 
                = PaymentOrdersRef.ForOrder(acumaticaOrderRef, SalesOrderType.SO, (double)paymentMinusRefundsAndDebits);

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
            refundPayment.PaymentRef = $"{transaction.id}".ToValue();
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


        private void PushPaymentAndWriteSync(ShopifyTransaction transactionRecord, PaymentWrite payment)
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
                paymentRecord.AcumaticaRefNbr 
                    = (resultPayment == null) 
                        ? AcumaticaSyncConstants.UnknownRefNbr : resultPayment.ReferenceNbr.value;

                paymentRecord.AcumaticaDocType = payment.Type.value;
                paymentRecord.AcumaticaAmount = (decimal)payment.PaymentAmount.value;
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

        private void PushReleaseAndUpdateSync(ShopifyTransaction transactionRecord)
        {
            var acumaticaPayment = transactionRecord.AcumaticaPayment;
            _paymentClient.ReleasePayment(acumaticaPayment.AcumaticaRefNbr, acumaticaPayment.AcumaticaDocType);
            _syncOrderRepository.PaymentIsReleased(acumaticaPayment.ShopifyTransactionMonsterId);
        }
    }
}


