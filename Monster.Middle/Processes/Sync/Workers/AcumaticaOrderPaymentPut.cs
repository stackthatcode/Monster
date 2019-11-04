﻿using System;
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
            var transactionsRecords = _syncOrderRepository.RetrieveUnsyncedTransactions();
            RunTransactions(transactionsRecords);
        }

        public void RunTransactions(IList<ShopifyTransaction> transactions)
        {
            foreach (var transaction in transactions)
            {
                var order = _syncOrderRepository.RetrieveShopifyOrder(transaction.ShopifyOrderId);
                var paymentTransaction = order.PaymentTransaction();

                var status = PaymentSyncStatus.Make(transaction, paymentTransaction);

                if (status.ShouldCreatePayment().Success)
                {
                    _logService.Log(LogBuilder.CreateAcumaticaPayment(transaction));

                    var payment = BuildPayment(transaction);
                    WritePaymentAndCreateSync(order, transaction, payment);
                    continue;
                }

                if (status.ShouldCreateRefundPayment().Success)
                {
                    _logService.Log(LogBuilder.CreateAcumaticaCustomerRefund(transaction));

                    var payment = BuildCustomerRefund(transaction);
                    WritePaymentAndCreateSync(order, transaction, payment);
                    continue;
                }
            }
        }

        private void WritePaymentAndCreateSync(
                   ShopifyOrder orderRecord, ShopifyTransaction transactionRecord, PaymentWrite payment)
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
    }
}


