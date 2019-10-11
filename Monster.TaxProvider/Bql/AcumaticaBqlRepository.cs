using System.Collections.Generic;
using System.Linq;
using Monster.TaxTransfer;
using Newtonsoft.Json;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.SO;

namespace Monster.TaxProvider.Bql
{
    public class AcumaticaBqlRepository
    {
        private readonly PXGraph _graph;
        private readonly Logger _logger = new Logger();

        public AcumaticaBqlRepository(PXGraph graph)
        {
            _graph = graph;
        }

        public PXResultset<ARTaxTran> RetrieveARTaxTransactions(
                    string orderType, string orderNbr, string taxId)
        {
            var taxTrans =
                PXSelectJoin<ARTaxTran,
                        InnerJoin<SOOrderShipment,
                            On<ARTaxTran.refNbr, Equal<SOOrderShipment.invoiceNbr>,
                                And<ARTaxTran.tranType, Equal<SOOrderShipment.invoiceType>>>>,
                        Where<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>,
                            And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>,
                            And<ARTaxTran.taxID, Equal<Required<ARTran.taxID>>>>>>
                    .Select(_graph, orderType, orderNbr, taxId);

            return taxTrans;
        }

        public SOOrder RetrieveSalesOrderByInvoice(string invoiceType, string invoiceNbr)
        {
            var soShipment =
                ((SOOrderShipment)PXSelect<SOOrderShipment,
                        Where<SOOrderShipment.invoiceNbr, Equal<Required<SOOrderShipment.invoiceNbr>>,
                            And<SOOrderShipment.invoiceType, Equal<Required<SOOrderShipment.invoiceType>>>>>
                    .Select(_graph, invoiceNbr, invoiceType));

            return RetrieveSalesOrder(soShipment.OrderType, soShipment.OrderNbr);
        }

        public SOOrder RetrieveSalesOrder(string orderType, string orderNumber)
        {
            var salesOrder =
                ((SOOrder)PXSelect<SOOrder,
                        Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
                            And<SOOrder.orderType, Equal<Required<SOOrder.orderType>>>>>
                    .Select(_graph, orderNumber, orderType));
            return salesOrder;
        }

        public Transfer RetrieveTaxTransfer(string orderType, string orderNumber)
        {
            var salesOrder = RetrieveSalesOrder(orderType, orderNumber);
            var salesOrderExt = PXCache<SOOrder>.GetExtension<SOOrderTaxSnapshotExt>(salesOrder);

            //var salesOrderExt = salesOrder.GetExtension<SOOrderTaxSnapshotExt>();
            _logger.Info("Tax Transfer loaded from Acumatica - " + salesOrderExt.UsrTaxSnapshot);

            return JsonConvert.DeserializeObject<Transfer>(salesOrderExt.UsrTaxSnapshot);
        }
    }
}
