using System.Linq;
using Monster.Middle.Persist.Instance;
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


        // Computed
        //
        public bool HasCreditMemoOrder => AcumaticaCreditMemoOrderNbr.HasValue();

        // Computed - action drivers

    }
}

