using System.Linq;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;

namespace Monster.Middle.Processes.Sync.Extensions
{
    public static class RefundExtensions
    {
        public static bool HasInvoicedShipment(this SalesOrder salesOrder)
        {
            return salesOrder.Shipments.Any(x => x.InvoiceNbr != null);
        }

        public static string ShipmentInvoiceNbr(this SalesOrder salesOrder)
        {
            return salesOrder.Shipments.First().InventoryRefNbr.value;
        }

        public static bool IsInvoiceReleased(this SalesOrder salesOrder)
        {
            return salesOrder.Status.value == AcumaticaConstants.StatusCompleted;
        }


        public static bool HasCreditMemoOrder(this UsrShopifyRefund refund)
        {
            return refund.UsrShopAcuRefundCms.Any();
        }

        public static bool DoesNotHaveCreditMemoOrder(this UsrShopifyRefund refund)
        {
            return !refund.HasCreditMemoOrder();
        }

        public static bool CreditMemoTaxesAreSynced(this UsrShopifyRefund refund)
        {
            return refund.UsrShopAcuRefundCms.Any() &&
                   refund.UsrShopAcuRefundCms.First().IsCmOrderTaxLoaded;
        }

        public static bool CreditMemoTaxesAreNotSynced(this UsrShopifyRefund refund)
        {
            return !refund.CreditMemoTaxesAreSynced();
        }

        public static bool HasCreditMemoInvoice(this UsrShopifyRefund refund)
        {
            return refund.UsrShopAcuRefundCms.Any() &&
                   refund.UsrShopAcuRefundCms.First().AcumaticaCreditMemoInvoiceNbr != null;
        }

        public static bool DoesNotHaveCreditMemoInvoice(this UsrShopifyRefund refund)
        {
            return !refund.HasCreditMemoInvoice();
        }

        public static bool HasReleasedInvoice(this UsrShopifyRefund refund)
        {
            return refund.UsrShopAcuRefundCms.Any() &&
                   refund.UsrShopAcuRefundCms.First().IsCmInvoiceReleased != null;
        }

        public static bool DoesNotHaveReleasedInvoice(this UsrShopifyRefund refund)
        {
            return !refund.HasReleasedInvoice();
        }

        public static bool IsReleased(this SalesInvoice input)
        {
            return input.Status.value == AcumaticaConstants.StatusOpen;
        }

        public static bool IsSyncComplete(this UsrShopifyRefund refund)
        {
            return refund.HasCreditMemoOrder()
                   && refund.CreditMemoTaxesAreSynced()
                   && refund.HasCreditMemoInvoice()
                   && refund.HasReleasedInvoice();
        }
    }
}
