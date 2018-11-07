using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
using Monster.Middle.Persist.Multitenant.Shopify;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaShipmentSync
    {
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        private readonly CustomerClient _customerClient;
        private readonly ShipmentClient _salesOrderClient;
        private readonly AcumaticaOrderPull _acumaticaOrderPull;

        public AcumaticaShipmentSync(
                ShopifyOrderRepository shopifyOrderRepository, 
                CustomerClient customerClient, 
                ShipmentClient salesOrderClient, 
                AcumaticaOrderPull acumaticaOrderPull)
        {
            _shopifyOrderRepository = shopifyOrderRepository;
            _customerClient = customerClient;
            _salesOrderClient = salesOrderClient;
            _acumaticaOrderPull = acumaticaOrderPull;
        }


        public void Run()
        {
            var shopifyFulfillments 
                    = _shopifyOrderRepository
                        .RetrieveFulfillmentsNotSynced();

            foreach (var fulfillment in shopifyFulfillments)
            {
                SyncFulfillmentWithAcumatica(fulfillment);
            }
        }

        //public void RunByShopifyId(long shopifyOrderId)
        //{
        //    var shopifyOrder = _orderRepository.RetrieveShopifyOrder(shopifyOrderId);
        //    SyncOrderWithAcumatica(shopifyOrder);
        //}

        private void SyncFulfillmentWithAcumatica(UsrShopifyFulfillment fulfillmentRecord)
        {
            var shopifyOrder
                = fulfillmentRecord
                    .UsrShopifyOrder
                    .ShopifyJson
                    .DeserializeToOrder();
            
            var shopifyFulfillment
                = shopifyOrder
                    .fulfillments
                    .FirstOrDefault(x => x.id == fulfillmentRecord.ShopifyFulfillmentId);

            var salesOrder = new SalesOrder();

            // TODO - convert this to a constant or configurable item
            //salesOrder.OrderType = "SO".ToValue();
            //salesOrder.Description = $"Shopify Order #{shopifyOrder.order_number}".ToValue();
            //salesOrder.CustomerID = customer.AcumaticaCustomerId.ToValue();
            //salesOrder.TaxTotal = ((double) shopifyOrder.total_tax).ToValue();
            //salesOrder.Details = new List<SalesOrderDetail>();
            
            //foreach (var lineItem in shopifyOrderRecord.UsrShopifyOrderLineItems)
            //{
            //    var stockItem
            //        = lineItem
            //            .UsrShopifyVariant
            //            .UsrAcumaticaStockItems
            //            .First();

            //    var shopifyLineItem
            //        = shopifyOrder
            //            .line_items
            //            .First(x => x.id == lineItem.ShopifyLineItemId);

            //    var salesOrderDetail = new SalesOrderDetail();

            //    salesOrderDetail.InventoryID = stockItem.ItemId.ToValue();

            //    salesOrderDetail.OrderQty 
            //        = ((double)shopifyLineItem.quantity).ToValue();

            //    salesOrderDetail.ExtendedPrice =
            //        ((double) shopifyLineItem.TotalAfterDiscount).ToValue();

            //    salesOrder.Details.Add(salesOrderDetail);
            //}

            //var resultJson 
            //    = _salesOrderClient.AddSalesOrder(salesOrder.SerializeToJson());

            //var resultSalesOrder 
            //    = resultJson.DeserializeFromJson<SalesOrder>();

            //var acumaticaRecord
            //    = _acumaticaOrderPull.UpsertOrderToPersist(resultSalesOrder);

            //acumaticaRecord.UsrShopifyOrder = shopifyOrderRecord;
            //_orderRepository.SaveChanges();
        }

        

        
    }
}

