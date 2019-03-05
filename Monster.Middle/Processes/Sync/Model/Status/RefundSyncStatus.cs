using System.Linq;
using Monster.Middle.Persist.Tenant;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class RefundSyncStatus
    {
        public string AcumaticaCreditMemoOrderNbr { get; set; }
        public bool AreCreditMemoTaxesSynced { get; set; }
        public string AcumaticaCreditMemoInvoiceNbr { get; set; }
        public bool HasCreditMemoInvoiceReleased { get; set; }


        public RefundSyncStatus()
        {
            AreCreditMemoTaxesSynced = false;
            HasCreditMemoInvoiceReleased = false;
        }

        public static RefundSyncStatus Make(UsrShopifyRefund refund)
        {
            var output = new RefundSyncStatus();
            var syncRecord = refund.UsrShopAcuRefundCms.FirstOrDefault();
            if (syncRecord == null)
            {
                return output;
            }

            output.AcumaticaCreditMemoOrderNbr = syncRecord.AcumaticaCreditMemoOrderNbr;
            output.AreCreditMemoTaxesSynced = syncRecord.IsCmOrderTaxLoaded;
            output.AcumaticaCreditMemoInvoiceNbr = syncRecord.AcumaticaCreditMemoInvoiceNbr;
            output.HasCreditMemoInvoiceReleased = syncRecord.IsCmInvoiceReleased;

            return output;
        }
        

        // Computed
        //
        public bool HasCreditMemoOrder => AcumaticaCreditMemoOrderNbr.HasValue();

        public bool DoesNotHaveCreditMemoOrder => !HasCreditMemoOrder;

        public bool AreCreditMemoTaxesNotSynced => !AreCreditMemoTaxesSynced;

        public bool HasCreditMemoInvoice => AcumaticaCreditMemoInvoiceNbr.HasValue();

        public bool DoesNotHaveCreditMemoInvoice => !HasCreditMemoInvoice;

        public bool DoesNotHaveReleasedInvoice
                    => DoesNotHaveCreditMemoInvoice || !HasCreditMemoInvoiceReleased;

        public bool IsSyncComplete
                    => HasCreditMemoOrder
                       && AreCreditMemoTaxesSynced
                       && HasCreditMemoInvoice
                       && HasCreditMemoInvoiceReleased;


        // Computed - action drivers

    }
}

