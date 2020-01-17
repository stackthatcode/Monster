using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Payment;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Misc.Acumatica;
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
using Push.Foundation.Utilities.Helpers;
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
        private readonly InvoiceClient _invoiceClient;
        private readonly SettingsRepository _settingsRepository;
        private readonly AcumaticaTimeZoneService _acumaticaTimeZoneService;
        private readonly PendingActionService _pendingActionService;
        private readonly IPushLogger _systemLogger;

        public AcumaticaOrderPaymentPut(
                    SyncOrderRepository syncOrderRepository, 
                    PaymentClient paymentClient, 
                    SettingsRepository settingsRepository, 
                    ExecutionLogService logService, 
                    PendingActionService pendingActionService, 
                    IPushLogger systemLogger, 
                    AcumaticaTimeZoneService acumaticaTimeZoneService, 
                    InvoiceClient invoiceClient)
        {
            _syncOrderRepository = syncOrderRepository;
            _paymentClient = paymentClient;
            _settingsRepository = settingsRepository;
            _logService = logService;
            _pendingActionService = pendingActionService;
            _systemLogger = systemLogger;
            _acumaticaTimeZoneService = acumaticaTimeZoneService;
            _invoiceClient = invoiceClient;
        }


        public void RunAutomatic()
        {
            var processingList = _syncOrderRepository.RetrieveSyncedOrdersWithUnsyncedTransactions();
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
                ProcessAllReleases(shopifyOrderId);

                // Refresh Status and run for Payment Transaction
                //
                ProcessPaymentTransaction(shopifyOrderId);
                //ProcessAllReleases(shopifyOrderId);

                // Refresh Status and run for Refund Transactions
                //
                var rootAction = _pendingActionService.Create(shopifyOrderId);
                foreach (var refundAction in rootAction.RefundPaymentActions)
                {
                    ProcessRefundTransaction(refundAction);
                    //ProcessAllReleases(shopifyOrderId);
                }

                var rootAction2 = _pendingActionService.Create(shopifyOrderId);
                foreach (var memoAction in rootAction2.AdjustmentMemoActions)
                {
                    ProcessAdjustmentMemo(memoAction);
                    //ProcessAllReleases(shopifyOrderId);
                }

                return true;
            }
            catch(Exception ex)
            {
                _systemLogger.Error(ex);
                _logService.Log($"Encounter error syncing Payments for Shopify Order {shopifyOrderId}");

                _syncOrderRepository.IncreaseOrderErrorCount(shopifyOrderId);
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

        private void ProcessAllReleases(long shopifyOrderId)
        {
            var status = _pendingActionService.Create(shopifyOrderId);

            ProcessTransactionRelease(status.PaymentAction);

            foreach (var refund in status.RefundPaymentActions)
            {
                ProcessTransactionRelease(refund);
            }

            foreach (var memo in status.AdjustmentMemoActions)
            {
                ProcessAdjustMemoRelease(memo);
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
                PushPaymentReleaseAndUpdateSync(transaction);
            }
        }


        public PaymentWrite BuildPaymentForCreate(ShopifyTransaction transactionRecord)
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
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrder(transactionRecord.OrderId());
            var order = orderRecord.ToShopifyObj();


            // Create the payload for Acumatica
            //
            var payment = new PaymentWrite();
            payment.CustomerID = acumaticaCustId.ToValue();
            payment.Hold = false.ToValue();
            payment.Type = PaymentType.Payment.ToValue();
            payment.PaymentRef = $"{transaction.id}".ToValue();

            var createdAtUtc = (transaction.created_at ?? order.created_at).UtcDateTime;
            var acumaticaDate = _acumaticaTimeZoneService.ToAcumaticaTimeZone(createdAtUtc);
            payment.ApplicationDate = acumaticaDate.Date.ToValue();

            // Amount computations
            //
            payment.PaymentAmount = ((double)transaction.amount).ToValue();
            var appliedToOrder = orderRecord.NetRemainingPayment();


            // Applied to Documents
            //
            var acumaticaOrderRef = orderRecord.AcumaticaSalesOrderId();
            if (acumaticaOrderRef.HasValue() && orderRecord.IsNotCancelledOrAllRefunded())
            {
                payment.OrdersToApply =
                    PaymentOrdersRef.ForOrder(acumaticaOrderRef, SalesOrderType.SO, (double) appliedToOrder);
            }

            payment.PaymentMethod = gateway.AcumaticaPaymentMethod.ToValue();
            payment.CashAccount = gateway.AcumaticaCashAccount.ToValue();
            payment.Description = $"Payment for Shopify Order #{orderRecord.ShopifyOrderNumber}".ToValue();

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
            //
            refundPayment.PaymentMethod = paymentGateway.AcumaticaPaymentMethod.ToValue();
            refundPayment.CashAccount = paymentGateway.AcumaticaCashAccount.ToValue();
            refundPayment.Description = $"Refund for Order #{order.ShopifyOrderNumber} (TransId #{transaction.id})".ToValue();

            return refundPayment;
        }

        private void PushPaymentAndWriteSync(ShopifyTransaction transactionRecord, PaymentWrite payment)
        {
            // Push to Acumatica
            //
            string resultJson;
            try
            {
                resultJson = _paymentClient.WritePayment(payment.SerializeToJson());
                _syncOrderRepository.ResetOrderErrorCount(transactionRecord.ShopifyOrderId);
            }
            catch (Exception ex)
            {
                _systemLogger.Error(ex);
                _logService.Log($"Encounter error syncing {transactionRecord.LogDescriptor()}");
                _syncOrderRepository.IncreaseOrderErrorCount(transactionRecord.ShopifyOrderId);
                return;
            }

            var resultPayment = resultJson.DeserializeFromJson<PaymentWrite>();
            var existingRecord = _syncOrderRepository.RetreivePayment(transactionRecord.Id);

            if (existingRecord == null)
            {
                // Create Monster Sync Record
                //
                var paymentRecord = new AcumaticaPayment();
                paymentRecord.ShopifyTransactionMonsterId = transactionRecord.Id;

                if (resultPayment == null)
                {
                    // Workaround for Acumatica Bug
                    //
                    paymentRecord.AcumaticaRefNbr = AcumaticaSyncConstants.UnknownRefNbr;
                }
                else
                {
                    paymentRecord.AcumaticaRefNbr = resultPayment.ReferenceNbr.value;
                }

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
            _syncOrderRepository.ResetOrderErrorCount(transactionRecord.ShopifyOrderId);
        }

        private void PushPaymentReleaseAndUpdateSync(ShopifyTransaction transactionRecord)
        {
            try
            {
                // Workarounds for Acumatica bug that prevents storage of Payment Nbr
                //
                var acumaticaPayment = RetrieveCorrectedPayment(transactionRecord);

                // Release the actual Payment
                //
                _paymentClient.ReleasePayment(acumaticaPayment.AcumaticaRefNbr, acumaticaPayment.AcumaticaDocType);
                _syncOrderRepository.PaymentIsReleased(acumaticaPayment.ShopifyTransactionMonsterId);
                _syncOrderRepository.ResetOrderErrorCount(transactionRecord.ShopifyOrderId);
            }
            catch (Exception ex)
            {
                _systemLogger.Error(ex);
                _logService.Log($"Encounter error syncing {transactionRecord.LogDescriptor()}");
                _syncOrderRepository.IncreaseOrderErrorCount(transactionRecord.ShopifyOrderId);
                return;
            }
        }

        private AcumaticaPayment RetrieveCorrectedPayment(ShopifyTransaction transactionRecord)
        {
            var acumaticaPayment = transactionRecord.AcumaticaPayment;
            if (acumaticaPayment.AcumaticaRefNbr == AcumaticaSyncConstants.UnknownRefNbr)
            {
                var paymentRef = transactionRecord.ShopifyTransactionId.ToString();
                var payments = _paymentClient.RetrievePaymentByPaymentRef(paymentRef);

                if (payments.Count == 0)
                {
                    _syncOrderRepository.DeleteErroneousPaymentRecord(transactionRecord.Id);
                    throw new Exception($"Shopify Transaction {transactionRecord.Id} sync to Acumatica Payment false record detected");
                }
                if (payments.Count > 1)
                {
                    throw new Exception($"Multiple Acumatica Payment records with Payment Ref {paymentRef}");
                }

                var correctedPaymentNbr = payments.First().ReferenceNbr.value;
                _syncOrderRepository.UpdatePaymentRecordRefNbr(transactionRecord.Id, correctedPaymentNbr);
                acumaticaPayment.AcumaticaRefNbr = correctedPaymentNbr;
            }

            return acumaticaPayment;
        }


        private void ProcessAdjustmentMemo(AdjustmentAction action)
        {
            if (action.ActionCode == ActionCode.None)
            {
                return;
            }

            if (action.ActionCode == ActionCode.CreateInAcumatica && action.IsValid)
            {
                // Create Acumatica Refund payload
                // 
                var refund = 
                    _syncOrderRepository.RetrieveShopifyRefundWithNoTracking(action.ShopifyRefundId);

                var memoWrite = BuildCreditMemo(refund);

                    //_invoiceClient.RetrieveInvoiceAndTaxes("000015", "CRM");

                // Push to Acumatica and write Sync Record
                //
                _logService.Log(LogBuilder.CreateAcumaticaMemo(refund));
                PushMemoAndWriteSync(refund, memoWrite);
            }
        }

        private void ProcessAdjustMemoRelease(AdjustmentAction action)
        {
            if (action.ActionCode == ActionCode.ReleaseInAcumatica && action.IsValid)
            {
                var refund = _syncOrderRepository.RetrieveShopifyRefundWithNoTracking(action.ShopifyRefundId);
                _logService.Log(LogBuilder.ReleaseAcumaticaMemo(refund));
                PushMemoReleaseAndUpdateSync(refund);
            }
        }

        private Invoice BuildCreditMemo(ShopifyRefund refund)
        {
            // Locate the Acumatica Customer
            //
            var acumaticaCustId = refund.ShopifyOrder.AcumaticaCustomerId();
            
            // Create the payload for Acumatica
            //
            var amount = ((double)refund.CreditAdjustment);
            
            var memo = new Invoice();
            memo.Customer = acumaticaCustId.ToValue();
            memo.CustomerOrder = refund.ShopifyRefundId.ToString().ToValue();
            memo.Hold = false.ToValue();
            memo.Type = SalesInvoiceType.Credit_Memo.ToValue();
            memo.Description = $"Adjustment for Shopify Refund {refund.ShopifyRefundId}".ToValue();
            memo.Amount = amount.ToValue();
            memo.Details = new List<object>()
            {
                new
                {
                    TransactionDescription = "Customer Credit".ToValue(),
                    ExtendedPrice = amount.ToValue(),
                }
            };

            return memo;
        }

        private void PushMemoAndWriteSync(ShopifyRefund refundRecord, Invoice invoice)
        {
            // Push to Acumatica
            //
            string resultJson;
            try
            {
                resultJson = _invoiceClient.WriteInvoice(invoice.SerializeToJson());
            }
            catch (Exception ex)
            {
                _systemLogger.Error(ex);
                _logService.Log($"Encounter error syncing memo for {refundRecord.LogDescriptor()}");
                return;
            }

            var result = resultJson.DeserializeFromJson<Invoice>();

            var existingRecord = _syncOrderRepository.RetreiveMemo(refundRecord.Id);
            if (existingRecord == null)
            {
                // Create Monster Sync Record
                //
                var newRecord = new AcumaticaMemo();
                newRecord.ShopifyRefundMonsterId = refundRecord.Id;

                if (result == null)
                {
                    // Workaround for Acumatica Bug
                    //
                    newRecord.AcumaticaRefNbr = AcumaticaSyncConstants.UnknownRefNbr;
                }
                else
                {
                    newRecord.AcumaticaRefNbr = result.ReferenceNbr.value;
                }

                newRecord.AcumaticaDocType = invoice.Type.value;
                newRecord.AcumaticaAmount = (decimal)invoice.Amount.value;
                newRecord.IsReleased = false;
                newRecord.IsAppliedToOrder = false;

                newRecord.DateCreated = DateTime.UtcNow;
                newRecord.LastUpdated = DateTime.UtcNow;
                _syncOrderRepository.InsertMemo(newRecord);
            }
            else
            {
                //existingRecord.AcumaticaAppliedToOrder = (decimal)payment.AmountAppliedToOrder;
                existingRecord.LastUpdated = DateTime.UtcNow;
                _syncOrderRepository.SaveChanges();
            }
            _syncOrderRepository.ResetOrderErrorCount(refundRecord.ShopifyOrderId);
        }

        private void PushMemoReleaseAndUpdateSync(ShopifyRefund refundRecord)
        {
            try
            {
                // Workarounds for Acumatica bug that prevents storage of Payment Nbr
                //
                var acumaticaMemo = RetrieveCorrectedCreditMemo(refundRecord);

                // Release the actual Payment
                //
                _invoiceClient.ReleaseInvoice(acumaticaMemo.AcumaticaRefNbr, acumaticaMemo.AcumaticaDocType);

                _syncOrderRepository.MemoIsReleased(acumaticaMemo.ShopifyRefundMonsterId);
                _syncOrderRepository.ResetOrderErrorCount(refundRecord.ShopifyOrderId);
            }
            catch (Exception ex)
            {
                _systemLogger.Error(ex);
                _logService.Log($"Encounter error syncing {refundRecord.LogDescriptor()}");
                _syncOrderRepository.IncreaseOrderErrorCount(refundRecord.ShopifyOrderId);
                return;
            }
        }

        private AcumaticaMemo RetrieveCorrectedCreditMemo(ShopifyRefund refundRecord)
        {
            var acumaticaCreditMemo = refundRecord.AcumaticaMemo;
            if (acumaticaCreditMemo.AcumaticaRefNbr == AcumaticaSyncConstants.UnknownRefNbr)
            {
                var customerOrder = refundRecord.ShopifyRefundId.ToString();
                var invoices = _invoiceClient.RetrieveInvoiceByCustomerOrder(customerOrder);

                if (invoices.Count == 0)
                {
                    _syncOrderRepository.DeleteMemoPaymentRecord(refundRecord.Id);
                    throw new Exception($"Shopify  {refundRecord.Id} sync to Acumatica Credit Memo false record detected");
                }
                if (invoices.Count > 1)
                {
                    throw new Exception($"Multiple Acumatica Memo records with Customer Order Number {customerOrder}");
                }

                var correctedReferenceNbr = invoices.First().ReferenceNbr.value;
                _syncOrderRepository.UpdateMemoRecordRefNbr(refundRecord.Id, correctedReferenceNbr);
                acumaticaCreditMemo.AcumaticaRefNbr = correctedReferenceNbr;
            }

            return acumaticaCreditMemo;
        }


        //memo.ApplicationsDefault = new List<object>()
        //{
        //    new
        //    {
        //        Amount = amount.ToValue(),
        //        DocType = SalesOrderType.SO.ToValue(),
        //        ReferenceNbr = refund.ShopifyOrder.AcumaticaSalesOrderId().ToValue(),
        //    }
        //};
    }
}

