﻿using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Payment;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Hangfire;
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
using Push.Shopify.Api.Order;


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
        private readonly ShopifyJsonService _shopifyJsonService;
        private readonly PendingActionService _pendingActionService;
        private readonly JobMonitoringService _jobMonitoringService;
        private readonly IPushLogger _systemLogger;


        public AcumaticaOrderPaymentPut(
                    SyncOrderRepository syncOrderRepository, 
                    PaymentClient paymentClient, 
                    SettingsRepository settingsRepository, 
                    ExecutionLogService logService, 
                    PendingActionService pendingActionService, 
                    JobMonitoringService jobMonitoringService,
                    AcumaticaTimeZoneService acumaticaTimeZoneService, 
                    InvoiceClient invoiceClient,
                    IPushLogger systemLogger, ShopifyJsonService shopifyJsonService)
        {
            _syncOrderRepository = syncOrderRepository;
            _paymentClient = paymentClient;
            _settingsRepository = settingsRepository;
            _pendingActionService = pendingActionService;
            _systemLogger = systemLogger;
            _shopifyJsonService = shopifyJsonService;
            _jobMonitoringService = jobMonitoringService;
            _acumaticaTimeZoneService = acumaticaTimeZoneService;
            _invoiceClient = invoiceClient;
            _logService = logService;
        }


        public void RunAutomatic()
        {
            var shopifyOrderIds = _syncOrderRepository.RetrieveOrdersWithPaymentsAndRefundsNeedingActions();
            
            foreach (var shopifyOrderId in shopifyOrderIds)
            {
                if (_jobMonitoringService.DetectCurrentJobInterrupt())
                {
                    return;
                }

                ProcessOrderAux(shopifyOrderId);
            }
        }

        private void ProcessOrderAux(long shopifyOrderId)
        {
            try
            {
                ProcessOrder(shopifyOrderId);
            }
            catch(Exception ex)
            {
                _systemLogger.Error(ex);
                _logService.Log($"Encounter error syncing Payments for Shopify Order {shopifyOrderId}");
                _syncOrderRepository.IncreaseOrderErrorCount(shopifyOrderId);
            }
        }

        public void ProcessOrder(long shopifyOrderId)
        {
            // Clear-out any un-Released Transaction
            //
            ProcessPaymentReleases(shopifyOrderId);

            // Refresh Status and run for Payment Transaction
            //
            ProcessPaymentTransaction(shopifyOrderId);
            ProcessPaymentReleases(shopifyOrderId);

            // Refresh Status and run for Refund Transactions
            //
            var rootAction = _pendingActionService.Create(shopifyOrderId);
            foreach (var refundAction in rootAction.RefundPaymentActions)
            {
                ProcessRefundTransaction(refundAction);
                ProcessPaymentReleases(shopifyOrderId);
            }

            var rootAction2 = _pendingActionService.Create(shopifyOrderId);
            foreach (var memoAction in rootAction2.AdjustmentMemoActions)
            {
                ProcessAdjustmentMemo(memoAction);
                ProcessAdjustmentReleases(shopifyOrderId);
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

        private void ProcessPaymentReleases(long shopifyOrderId)
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
                PushPaymentReleaseAndUpdateSync(transaction);
            }
        }


        public PaymentWrite BuildPaymentForCreate(ShopifyTransaction transactionRecord)
        {
            var transaction = _shopifyJsonService.RetrieveTransaction(transactionRecord.ShopifyTransactionId);
            var gateway = _settingsRepository.RetrievePaymentGatewayByShopifyId(transaction.gateway);

            // Locate the Acumatica Customer
            //
            var shopifyCustomerId = transactionRecord.CustomerId();
            var customer = _syncOrderRepository.RetrieveCustomer(shopifyCustomerId);
            var acumaticaCustId = customer.AcumaticaCustId();

            // Build the Payment Ref and Description
            //
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrder(transactionRecord.OrderId());
            var order = _shopifyJsonService.RetrieveOrder(orderRecord.ShopifyOrderId);

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
            var appliedToOrder = orderRecord.TheoreticalPaymentRemaining();


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

            // Get the balance from Acumatica in case other Invoices have grabbed some of the monies!!
            // 
            var balance = _paymentClient.RetrievePaymentBalance(paymentNbr, PaymentType.Payment);
            var amountToApply 
                = balance < order.TheoreticalPaymentRemaining()
                    ? balance : order.TheoreticalPaymentRemaining();

            var payment = new PaymentWrite();
            payment.ReferenceNbr = paymentNbr.ToValue();
            payment.Type = PaymentType.Payment.ToValue();
            payment.OrdersToApply 
                = PaymentOrdersRef.ForOrder(acumaticaOrderRef, SalesOrderType.SO, (double)amountToApply);

            return payment;
        }

        private PaymentWrite BuildCustomerRefund(ShopifyTransaction transactionRecord)
        {
            var transaction = _shopifyJsonService.RetrieveTransaction(transactionRecord.ShopifyTransactionId);
            var paymentGateway = _settingsRepository.RetrievePaymentGatewayByShopifyId(transaction.gateway);

            // Locate the Acumatica Customer
            //
            var shopifyCustomerId = transactionRecord.CustomerId();
            var customer = _syncOrderRepository.RetrieveCustomer(shopifyCustomerId);
            var acumaticaCustId = customer.AcumaticaCustId();

            // Build the Payment Ref and Description
            //
            var order = _syncOrderRepository.RetrieveShopifyOrder(transactionRecord.OrderId());

            // Create the payload for Acumatica
            //
            var refundPayment = new PaymentWrite();
            refundPayment.CustomerID = acumaticaCustId.ToValue();
            refundPayment.Type = PaymentType.CustomerRefund.ToValue();
            refundPayment.PaymentRef = $"{transaction.id}".ToValue();

            // Reference to the original Payment
            //
            if (transactionRecord.NeedManualApply())
            {
                refundPayment.Hold = true.ToValue();
                refundPayment.PaymentAmount = ((double)transaction.amount).ToValue();
            }
            else
            {
                var acumaticaPayment = order.PaymentTransaction().AcumaticaPayment;
                var balance = _paymentClient.RetrievePaymentBalance(acumaticaPayment.AcumaticaRefNbr, PaymentType.Payment);

                var amountToApply = transaction.amount > balance ? balance : transaction.amount;

                refundPayment.Hold = false.ToValue();
                refundPayment.PaymentAmount = ((double)amountToApply).ToValue();
                refundPayment.DocumentsToApply
                    = PaymentDocumentsToApply.ForDocument(
                        acumaticaPayment.AcumaticaRefNbr, acumaticaPayment.AcumaticaDocType, (double)amountToApply);
            }

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
            var existingRecord = _syncOrderRepository.RetreivePayment(transactionRecord.MonsterId);

            if (existingRecord == null)
            {
                // Create Monster Sync Record
                //
                var paymentRecord = new AcumaticaPayment();
                paymentRecord.ShopifyTransactionMonsterId = transactionRecord.MonsterId;

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

                if (transactionRecord.IsRefund())
                {
                    if (transactionRecord.NeedManualApply())
                    {
                        // Users are tasked with creating their Return for Credit
                        // 
                        paymentRecord.NeedRelease = false;
                        paymentRecord.NeedManualApply = true;
                    }
                    else
                    {
                        paymentRecord.NeedRelease = true;
                        paymentRecord.NeedManualApply = false;
                    }
                }

                if (!transactionRecord.IsRefund())
                {
                    paymentRecord.NeedRelease = true;
                    paymentRecord.NeedManualApply = false;
                }

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

            _syncOrderRepository.UpdateShopifyOriginalPaymentNeedPut(transactionRecord.ShopifyOrderMonsterId, false);
            _syncOrderRepository.ResetOrderErrorCount(transactionRecord.ShopifyOrderId);
        }

        private void PushPaymentReleaseAndUpdateSync(ShopifyTransaction transactionRecord)
        {
            try
            {
                // Workarounds for Acumatica bug that prevents storage of Payment Nbr
                //
                var acumaticaPayment = RetrievePaymentWithMissingId(transactionRecord);

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

        private AcumaticaPayment RetrievePaymentWithMissingId(ShopifyTransaction transactionRecord)
        {
            var acumaticaPayment = transactionRecord.AcumaticaPayment;
            if (acumaticaPayment.AcumaticaRefNbr == AcumaticaSyncConstants.UnknownRefNbr)
            {
                var paymentRef = transactionRecord.ShopifyTransactionId.ToString();
                var payments = _paymentClient.RetrievePaymentByPaymentRef(paymentRef);

                if (payments.Count == 0)
                {
                    _syncOrderRepository.DeleteErroneousPaymentRecord(transactionRecord.MonsterId);
                    throw new Exception($"Shopify Transaction {transactionRecord.MonsterId} sync to Acumatica Payment false record detected");
                }
                if (payments.Count > 1)
                {
                    throw new Exception($"Multiple Acumatica Payment records with Payment Ref {paymentRef}");
                }

                var correctedPaymentNbr = payments.First().ReferenceNbr.value;
                _syncOrderRepository.UpdatePaymentRecordRefNbr(transactionRecord.MonsterId, correctedPaymentNbr);
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

            if (action.ActionCode == ActionCode.CreateInAcumatica
                    && action.MemoType ==  AdjustmentMemoType.CreditMemo && action.IsValid)
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

        private void ProcessAdjustmentReleases(long shopifyOrderId)
        {
            var status = _pendingActionService.Create(shopifyOrderId);

            foreach (var memo in status.AdjustmentMemoActions)
            {
                ProcessAdjustMemoRelease(memo);
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

            var existingRecord = _syncOrderRepository.RetreiveMemo(refundRecord.MonsterId);
            if (existingRecord == null)
            {
                // Create Monster Sync Record
                //
                var newRecord = new AcumaticaMemo();
                newRecord.ShopifyRefundMonsterId = refundRecord.MonsterId;

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
                newRecord.NeedRelease = true;
                newRecord.NeedManualApply = true;

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
                var acumaticaMemo = RetrieveCreditMemoWithMissingId(refundRecord);

                // Release the actual Memo
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

        private AcumaticaMemo RetrieveCreditMemoWithMissingId(ShopifyRefund refundRecord)
        {
            var acumaticaCreditMemo = refundRecord.AcumaticaMemo;
            if (acumaticaCreditMemo.AcumaticaRefNbr == AcumaticaSyncConstants.UnknownRefNbr)
            {
                var customerOrder = refundRecord.ShopifyRefundId.ToString();
                var invoices = _invoiceClient.RetrieveInvoiceByCustomerOrder(customerOrder);

                if (invoices.Count == 0)
                {
                    _syncOrderRepository.DeleteMemoPaymentRecord(refundRecord.MonsterId);
                    throw new Exception(
                        $"Shopify  {refundRecord.MonsterId} sync to Acumatica Credit Memo false record detected");
                }

                if (invoices.Count > 1)
                {
                    throw new Exception($"Multiple Acumatica Memo records with Customer Order Number {customerOrder}");
                }

                var correctedReferenceNbr = invoices.First().ReferenceNbr.value;
                _syncOrderRepository.UpdateMemoRecordRefNbr(refundRecord.MonsterId, correctedReferenceNbr);
                acumaticaCreditMemo.AcumaticaRefNbr = correctedReferenceNbr;
            }

            return acumaticaCreditMemo;
        }

    }
}

