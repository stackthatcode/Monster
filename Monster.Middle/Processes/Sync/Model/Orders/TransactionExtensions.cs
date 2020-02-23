using System;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;


namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class TransactionExtensions
    {
        public static bool ExistsInAcumatica(this ShopifyTransaction transaction)
        {
            return transaction.AcumaticaPayment != null;
        }

        public static bool IsReleased(this ShopifyTransaction transaction)
        {
            return transaction.AcumaticaPayment != null && !transaction.AcumaticaPayment.NeedRelease;
        }

        public static bool NeedsSync(this ShopifyTransaction transaction)
        {
            return transaction.IsSyncableToPayment 
                   && (!transaction.ExistsInAcumatica() || !transaction.IsReleased());
        }

        public static bool OriginalPaymentNeedsUpdateForRefund(this ShopifyOrder order)
        {
            return order.ShopifyRefunds.Any(x => x.NeedOriginalPaymentPut);
        }

        public static bool HasShippingRefund(this ShopifyTransaction transaction)
        {
            return transaction.ShopifyRefundId.HasValue &&
                transaction.ShopifyOrder.Refund(transaction.ShopifyRefundId.Value).HasShipping();
        }

        public static bool HasShipping(this ShopifyRefund refund)
        {
            return (Math.Abs(refund.Shipping) + Math.Abs(refund.ShippingTax)) > 0;
        }

        public static bool NeedManualApply(this ShopifyTransaction record)
        {
            return record.IsPureCancel ||
                   (record.HasShippingRefund() && record.ShopifyOrder.HasInvoicedShipments());
        }
    }
}

