﻿using System;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Payment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Sync.Orders.Workers
{
    public class AcumaticaPaymentSync
    {
        private readonly TenantRepository _tenantRepository;
        private readonly JobRepository _jobRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly PaymentClient _paymentClient;

        public AcumaticaPaymentSync(
                TenantRepository tenantRepository,
                SyncOrderRepository syncOrderRepository, 
                PaymentClient paymentClient, 
                JobRepository jobRepository)
        {
            _tenantRepository = tenantRepository;
            _syncOrderRepository = syncOrderRepository;
            _paymentClient = paymentClient;
            _jobRepository = jobRepository;
        }

        public void Run()
        {
            var unsyncedTransactions 
                    = _syncOrderRepository.RetrieveUnsyncedTransactions();

            foreach (var transaction in unsyncedTransactions)
            {
                WritePayment(transaction);
            }
        }

        public void WritePayment(UsrShopifyTransaction transactionRecord)
        {
            // Arrange data from local cache
            var preferences = _tenantRepository.RetrievePreferences();
            var paymentMethod = preferences.AcumaticaPaymentMethod;

            var shopifyCustomerId
                = transactionRecord
                    .UsrShopifyOrder.UsrShopifyCustomer.ShopifyCustomerId;

            var customer = _syncOrderRepository.RetrieveCustomer(shopifyCustomerId);
            var acumaticaCustomerId = customer.MatchingCustomer().AcumaticaCustomerId;

            // Extract the Transaction Amount
            var transaction 
                = transactionRecord
                    .ShopifyJson
                    .DeserializeFromJson<Transaction>();
            var paymentAmount = transaction.amount;

            // Build the Payment Ref and Description
            var order = _syncOrderRepository.RetrieveShopifyOrder(transactionRecord.UsrShopifyOrder.ShopifyOrderId);
            var acumaticaOrderRef = order.MatchingSalesOrder().AcumaticaOrderNbr;
            var paymentRef = $"{acumaticaOrderRef}".ToValue();
            var description = $"Payment for Shopify Order #{order.ShopifyOrderNumber}";

            // Create the payload for Acumatica
            var payment = new PaymentWrite();
            payment.CustomerID = acumaticaCustomerId.ToValue();
            payment.Type = "Payment".ToValue();
            payment.PaymentMethod = paymentMethod.ToValue();
            payment.CashAccount = preferences.AcumaticaPaymentCashAccount.ToValue();
            payment.PaymentRef = paymentRef;
            payment.Description = description.ToValue();
            payment.PaymentAmount = ((double)paymentAmount).ToValue();

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
                      $"from Shopify Order #{order.ShopifyOrderNumber}";
            _jobRepository.InsertExecutionLog(log);
        }
    }
}