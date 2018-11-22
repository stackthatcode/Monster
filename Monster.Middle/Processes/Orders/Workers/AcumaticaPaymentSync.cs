using System;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Payment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Sync;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaPaymentSync
    {
        private readonly TenantRepository _tenantRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly PaymentClient _paymentClient;

        public AcumaticaPaymentSync(
                TenantRepository tenantRepository,
                SyncOrderRepository syncOrderRepository, 
                PaymentClient paymentClient)
        {
            _tenantRepository = tenantRepository;
            _syncOrderRepository = syncOrderRepository;
            _paymentClient = paymentClient;
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

        public void WritePayment(UsrShopifyTransaction transaction)
        {
            // Arrange data from local cache
            var preferences = _tenantRepository.RetrievePreferences();
            var paymentMethod = preferences.AcumaticaPaymentMethod;

            var shopifyCustomerId
                = transaction
                    .UsrShopifyOrder.UsrShopifyCustomer.ShopifyCustomerId;

            var customer = _syncOrderRepository.RetrieveCustomer(shopifyCustomerId);
            var acumaticaCustomerId = customer.MatchingCustomer().AcumaticaCustomerId;

            // Create the payload for Acumatica
            var payment = new PaymentWrite();
            payment.CustomerID = acumaticaCustomerId.ToValue();
            payment.Type = "Payment".ToValue();
            payment.PaymentMethod = paymentMethod.ToValue();

            // Push to Acumatica
            var resultJson = _paymentClient.WritePayment(paymentMethod);
            var resultPayment = resultJson.DeserializeFromJson<PaymentWrite>();
            var paymentNbr = resultPayment.ReferenceNbr.value;

            // Create Monster Sync Record
            var paymentRecord = new UsrShopifyAcuPayment();
            paymentRecord.UsrShopifyTransaction = transaction;
            paymentRecord.ShopifyPaymentNbr = paymentNbr;
            paymentRecord.DateCreated = DateTime.UtcNow;
            paymentRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.InsertPayment(paymentRecord);
        }
    }
}
