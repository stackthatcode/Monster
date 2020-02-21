using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Analysis;
using Monster.Middle.Processes.Sync.Model.Orders;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public static class TransactionExtensions
    {
        public static bool IsMatch(this ShopifyTransaction input, ShopifyTransaction other)
        {
            return input.ShopifyTransactionId == other.ShopifyTransactionId;
        }

        public static ShopifyTransaction Find(this IEnumerable<ShopifyTransaction> input, ShopifyTransaction other)
        {
            return input.FirstOrDefault(x => x.IsMatch(other));
        }

        public static long CustomerId(this ShopifyTransaction transactionRecord)
        {
            return transactionRecord.ShopifyOrder.ShopifyCustomer.ShopifyCustomerId;
        }

        public static long OrderId(this ShopifyTransaction transactionRecord)
        {
            return transactionRecord.ShopifyOrder.ShopifyOrderId;
        }



        // Monster Transaction record extensions
        //
        public static bool IsPayment(this ShopifyTransaction transaction)
        {
            return transaction.ShopifyKind == TransactionKind.Capture
                   || transaction.ShopifyKind == TransactionKind.Sale;
        }

        public static bool IsRefund(this ShopifyTransaction transaction)
        {
            return transaction.ShopifyKind == TransactionKind.Refund;
        }

        public static bool DoNotIgnore(this ShopifyTransaction transaction)
        {
            return transaction.ShopifyGateway != Gateway.Manual
                   && transaction.ShopifyStatus == TransactionStatus.Success
                   && (transaction.IsPayment() || transaction.IsRefund());
        }

        public static bool Ignore(this ShopifyTransaction transaction)
        {
            return !transaction.DoNotIgnore();
        }

        public static ShopifyTransaction PaymentTransaction(this ShopifyOrder order)
        {
            return order.ShopifyTransactions.FirstOrDefault(x => x.DoNotIgnore() && x.IsPayment());
        }

        public static AcumaticaPayment AcumaticaPayment(this ShopifyOrder order)
        {
            return order.PaymentTransaction() != null ? order.PaymentTransaction().AcumaticaPayment : null;
        }


        public static bool HasPayment(this ShopifyOrder order)
        {
            return order.PaymentTransaction() != null;
        }

        public static List<ShopifyTransaction> RefundTransactions(this ShopifyOrder order, bool excludePureReturns = false)
        {
            var output = order.ShopifyTransactions.Where(x => x.DoNotIgnore() && x.IsRefund());

            if (excludePureReturns)
            {
                output = output.Where(x => x.IsPureReturn == false);
            }
            return output.ToList();
        }

        public static List<ShopifyTransaction> UnreleasedTransaction(this ShopifyOrder order)
        {
            return order.ShopifyTransactions
                    .Where(x => x.DoNotIgnore() && x.ExistsInAcumatica() && !x.IsReleased())
                    .ToList();
        }

        public static bool HasUnreleasedTransactions(this ShopifyOrder order)
        {
            return order.UnreleasedTransaction().Any();
        }


        // Payment summarization functions
        //

        public static decimal ShopifyPaymentAmount(this ShopifyOrder order)
        {
            return order.PaymentTransaction() != null
                ? order.PaymentTransaction().ShopifyAmount
                : 0m;
        }

        public static decimal ShopifyRefundTotal(this ShopifyOrder order,bool excludePureReturns = false)
        {
            return order.RefundTransactions(excludePureReturns).Sum(x => x.ShopifyAmount);
        }

        public static decimal ShopifyNetPayment(this ShopifyOrder order, bool excludePureReturns = false)
        {
            return order.ShopifyPaymentAmount() - order.ShopifyRefundTotal();
        }


        // These should be obsoleted... but they are fine for now
        //
        public static decimal AcumaticaPaymentTotal(this ShopifyOrder order)
        {
            return order.AcumaticaPayment() != null 
                ? order.AcumaticaPayment().AcumaticaAmount : 0m;
        }

        public static decimal AcumaticaRefundTotal(this ShopifyOrder order)
        {
            return order.RefundTransactions()
                .Where(x => x.AcumaticaPayment != null)
                .Select(x => x.AcumaticaPayment).Sum(x => x.AcumaticaAmount);
        }



        // *** IMPORTANT => Shopify is the single source of truth (SSoT) for Payment Transactions
        //  ... whereas Acumatica is the SSoT for goods issued to Customer via Shipments
        //
        public static decimal TheoreticalPaymentRemaining(this ShopifyOrder order)
        {
            // SAVE => only change this with careful and thorough investigation
            //
            var output = order.ShopifyNetPayment(excludePureReturns:true) - order.AcumaticaInvoiceTotal();
            return output <= 0.00m ? 0.00m : output;
        }



        // Shopify Transaction API DTO
        //
        public static bool IsPayment(this Transaction transaction)
        {
            return transaction.kind == TransactionKind.Capture 
                   || transaction.kind == TransactionKind.Sale;
        }

        public static bool IsRefund(this Transaction transaction)
        {
            return transaction.kind == TransactionKind.Refund;
        }

        public static bool DoNotIgnore(this Transaction transaction)
        {
            return transaction.gateway != Gateway.Manual
                   && transaction.status == TransactionStatus.Success
                   && (transaction.IsPayment() || transaction.IsRefund());
        }

        public static bool Ignore(this Transaction transaction)
        {
            return !transaction.DoNotIgnore();
        }

        public static List<ShopifyRefund> CreditAdustmentRefunds(this ShopifyOrder orderRecord)
        {
            return orderRecord.ShopifyRefunds.Where(x => x.CreditAdjustment > 0m).ToList();
        }

        public static List<ShopifyRefund> DebitAdustmentRefunds(this ShopifyOrder orderRecord)
        {
            return orderRecord.ShopifyRefunds.Where(x => x.DebitAdjustment > 0m).ToList();
        }

        public static List<AcumaticaSoShipment> SoShipments(this ShopifyOrder orderRecord)
        {
            return orderRecord.AcumaticaSalesOrder == null
                   ? new List<AcumaticaSoShipment>()
                   : orderRecord.AcumaticaSalesOrder.AcumaticaSoShipments.ToList();
        }
    }
}

