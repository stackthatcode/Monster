using System;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Payment;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Extensions;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Sync.Workers.Orders
{
    public class AcumaticaOrderPaymentSync
    {
        private readonly ExecutionLogService _logService;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly PaymentClient _paymentClient;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly PreferencesRepository _preferencesRepository;

        public AcumaticaOrderPaymentSync(
                    SyncOrderRepository syncOrderRepository, 
                    PaymentClient paymentClient, 
                    PreferencesRepository preferencesRepository, 
                    ExecutionLogService logService,
                    SalesOrderClient salesOrderClient)
        {
            _syncOrderRepository = syncOrderRepository;
            _paymentClient = paymentClient;
            _preferencesRepository = preferencesRepository;
            _logService = logService;
            _salesOrderClient = salesOrderClient;
        }

        public void RunPaymentsForOrders()
        {
            var transactions = _syncOrderRepository.RetrieveUnsyncedPayments();

            foreach (var transaction in transactions)
            {
                var status = PaymentSyncStatus.Make(transaction);

                if (status.ShouldCreatePayment().Success)
                {
                    _logService.ExecuteWithFailLog(
                            () => WritePaymentForOrders(transaction),
                            SyncDescriptor.CreateAcumaticaPayment,
                            SyncDescriptor.ShopifyTransaction(transaction));
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
            _logService.InsertExecutionLog($"Created {payment.Description.value}");
        }

        
        public void RunPaymentsForRefunds()
        {
            var transactions = _syncOrderRepository.RetrieveUnsyncedRefunds();

            foreach (var transaction in transactions)
            {
                var refund = 
                    _syncOrderRepository.RetrieveRefundAndSync(transaction.ShopifyRefundId.Value);

                // Cancellation
                //
                if (refund.ShopifyIsCancellation)
                {
                    WriteRefundPayment(transaction);
                    continue;
                }
                
                // Return/Restock Refund - Credit Memo Order and Invoice are created
                //
                if (refund.UsrShopAcuRefundCms.Any(x => x.IsComplete))
                {
                    WriteRefundPayment(transaction);
                }
            }
        }
        
        public void WriteRefundPayment(UsrShopifyTransaction transactionRecord)
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var transaction = transactionRecord.ShopifyJson.DeserializeFromJson<Transaction>();
            var refund = _syncOrderRepository
                    .RetrieveRefundAndSync(transactionRecord.ShopifyRefundId.Value);

            
            // Locate the Acumatica Customer
            var shopifyCustomerId = transactionRecord.CustomerId();
            var customer = _syncOrderRepository.RetrieveCustomer(shopifyCustomerId);
            var acumaticaCustId = customer.AcumaticaCustId();

            // Build the Payment Ref and Description
            var order = _syncOrderRepository.RetrieveShopifyOrder(transactionRecord.OrderId());
            

            // Create the JSON for Acumatica
            var payment = new PaymentWrite();
            payment.CustomerID = acumaticaCustId.ToValue();
            payment.Type = PaymentType.CustomerRefund.ToValue();
            payment.PaymentRef = $"{order.ShopifyOrderNumber}".ToValue();

            // Amounts
            payment.PaymentAmount = ((double)transaction.amount).ToValue();

            if (refund.UsrShopAcuRefundCms.Any())
            {
                var refundSync = refund.UsrShopAcuRefundCms.First();
                
                var salesInvoice =
                    _salesOrderClient.RetrieveSalesOrderInvoice(
                            refundSync.AcumaticaCreditMemoInvoiceNbr, SalesInvoiceType.Credit_Memo)
                        .DeserializeFromJson<SalesInvoice>();

                payment.DocumentsToApply =
                    PaymentDocumentsToApply.ForDocument(
                        refundSync.AcumaticaCreditMemoInvoiceNbr,
                        SalesInvoiceType.Credit_Memo,
                        salesInvoice.Amount.value);

                payment.AppliedToDocuments = ((double) salesInvoice.Amount.value).ToValue();

                var paymentNotEqualToInvoice
                    = (double) transaction.amount != salesInvoice.Amount.value;
                payment.Hold = paymentNotEqualToInvoice.ToValue();
            }

            // TODO - inject the original Payment Method...?
            // 
            payment.PaymentMethod = preferences.AcumaticaPaymentMethod.ToValue();
            payment.CashAccount = preferences.AcumaticaPaymentCashAccount.ToValue();
            payment.Description =
                $"Refund for Order #{order.ShopifyOrderNumber} (TransId #{transaction.id})".ToValue();

            // Push to Acumatica
            //
            var resultJson = _paymentClient.WritePayment(payment.SerializeToJson());
            var resultPayment = resultJson.DeserializeFromJson<PaymentWrite>();

            // Create Monster Sync Record
            //
            var paymentRecord = new UsrShopifyAcuPayment();
            paymentRecord.UsrShopifyTransaction = transactionRecord;
            paymentRecord.ShopifyPaymentNbr = resultPayment.ReferenceNbr.value;
            paymentRecord.AcumaticaPaymentType = resultPayment.Type.value;
            paymentRecord.DateCreated = DateTime.UtcNow;
            paymentRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.InsertPayment(paymentRecord);

            // Write Execution Log
            _logService.InsertExecutionLog($"Created {payment.Description.value}");
        }
    }
}


