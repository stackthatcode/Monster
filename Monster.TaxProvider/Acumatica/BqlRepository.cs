using System;
using Monster.TaxProvider.Utility;
using Monster.TaxTransfer;
using Monster.TaxTransfer.v2;
using Newtonsoft.Json;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.SO;

namespace Monster.TaxProvider.Acumatica
{
    public class BqlRepository
    {
        private readonly PXGraph _graph;
        private readonly Logger _logger;
            
        public BqlRepository(PXGraph graph, Logger logger)
        {
            _graph = graph;
            _logger = logger;
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

        public PXResultset<ARTaxTran> RetrieveARTaxTransactions(string orderType, string orderNbr)
        {
            var taxTrans =
                PXSelectJoin<ARTaxTran,
                        InnerJoin<SOOrderShipment,
                            On<ARTaxTran.refNbr, Equal<SOOrderShipment.invoiceNbr>,
                                And<ARTaxTran.tranType, Equal<SOOrderShipment.invoiceType>>>>,
                        Where<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>,
                            And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>>>>
                    .Select(_graph, orderType, orderNbr);

            return taxTrans;
        }

        public SOOrder RetrieveSalesOrder(string orderType, string orderNumber)
        {
            var salesOrder =
                ((SOOrder) PXSelect<SOOrder,
                        Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
                            And<SOOrder.orderType, Equal<Required<SOOrder.orderType>>>>>
                    .Select(_graph, orderNumber, orderType));
            return salesOrder;
        }

        public TaxSnapshot RetrieveTaxSnapshot(string orderType, string orderNumber)
        {
            var salesOrder = RetrieveSalesOrder(orderType, orderNumber);
            var salesOrderExt = PXCache<SOOrder>.GetExtension<SOOrderTaxSnapshotExt>(salesOrder);
            //var salesOrderExt = salesOrder.GetExtension<SOOrderTaxSnapshotExt>();

            if (salesOrderExt.UsrTaxSnapshot == null || salesOrderExt.UsrTaxSnapshot.Trim() == String.Empty)
            {
                throw new Exception("Missing Tax Snapshot - unable to compute Taxes");
            }

            // Unpack Base64-encoded and GZip-compressed tax data
            //
            string serializedData = salesOrderExt.UsrTaxSnapshot.Unzip();

            _logger.Debug($"Tax Snapshot ({orderType} {orderNumber}) - " + serializedData);

            return serializedData.DeserializeTaxSnapshot();
        }
    }
}
