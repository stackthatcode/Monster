using System.Linq;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Shopify;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;

namespace Monster.Middle.Processes.Orders.Workers
{
    public class ShopifyFulfillmentSync
    {
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;
        private readonly OrderApi _orderApi;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;

        // Possibly expand - this is a one-time thing...
        public const int InitialBatchStateFudgeMin = -15;


        public ShopifyFulfillmentSync(
                IPushLogger logger,
                BatchStateRepository batchStateRepository,
                OrderApi orderApi,
                ShopifyOrderRepository shopifyOrderRepository,
                AcumaticaOrderRepository acumaticaOrderRepository)
        {
            _logger = logger;
            _batchStateRepository = batchStateRepository;
            _orderApi = orderApi;
            _shopifyOrderRepository = shopifyOrderRepository;
            _acumaticaOrderRepository = acumaticaOrderRepository;
        }


        public void Run()
        {
            // Shipments that have Sales Orders, but no matching Shopify Fulfillment
            var shipmentsRecords =
                _acumaticaOrderRepository.RetrieveShipmentsUnsynced();

            // TODO - more Acumatica Status filtering
            var correctedShipmentRecords =
                shipmentsRecords
                    .Where(x => x.AcumaticaStatus != "Hold")
                    .ToList();

            foreach (var shipmentRecord in correctedShipmentRecords)
            {
                var shipment = 
                    shipmentRecord
                        .AcumaticaJson
                        .DeserializeFromJson<Shipment>();

                foreach (var orderNbr in shipment.UniqueOrderNbrs)
                {
                    var order = 
                        _acumaticaOrderRepository.RetrieveSalesOrder(orderNbr);

                    //if (order.UsrShopifyOrder == null)
                    //{
                    //    continue;
                    //}

                }
            }
        }

        public void PushFulfillmentToShopify()
        {

        }
    }
}
