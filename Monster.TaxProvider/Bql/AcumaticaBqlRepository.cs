using Monster.TaxTransfer;
using Newtonsoft.Json;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.SO;
using PX.SM;

namespace Monster.TaxProvider.Bql
{
    public class AcumaticaBqlRepository
    {
        private readonly PXGraph _graph;

        public AcumaticaBqlRepository(PXGraph graph)
        {
            _graph = graph;
        }


        public Transfer RetrieveTaxTransfer(string orderType, string orderNumber)
        {
            var salesOrder =
                ((SOOrder) PXSelect<SOOrder,
                        Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
                            And<SOOrder.orderType, Equal<Required<SOOrder.orderType>>>>>
                    .Select(_graph, orderNumber, orderType));

            var salesOrderExt = PXCache<SOOrder>.GetExtension<SOOrderTaxSnapshotExt>(salesOrder);
            //var salesOrderExt = salesOrder.GetExtension<SOOrderTaxSnapshotExt>();
            
            return JsonConvert.DeserializeObject<Transfer>(salesOrderExt.UsrTaxSnapshot);
        }


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
