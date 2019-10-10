using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.SO;

namespace Monster.TaxProvider
{
    public class AcumaticaBqlRepository
    {
        public void DoItNow()
        {
            var graph = new PXGraph();

            var shipment =
                ((SOOrderShipment)
                    PXSelect<SOOrderShipment,
                            Where<SOOrderShipment.invoiceNbr,
                                Equal<Required<SOOrderShipment.invoiceNbr>>>>
                        .Select(graph, "000016"));
            //.InvoiceType;

            var taxTrans = (
                PXSelect<ARTaxTran, Where<ARTaxTran.refNbr,
                    Equal<Required<ARTaxTran.refNbr>>>>.Select(graph, "000018"));

            foreach (var trans in taxTrans)
            {

            }

            var salesOrder =
                ((SOOrder)PXSelect<SOOrder,
                        Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>
                    .Select(graph, "000004"));
            //var salesOrderExt = salesOrder.GetExtension<SOOrderTaxSnapshotExt>();

            var salesOrderExt = PXCache<SOOrder>.GetExtension<SOOrderTaxSnapshotExt>(salesOrder);
            var snapshot = salesOrderExt.UsrTaxSnapshot;
        }
    }
}
