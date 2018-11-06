using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaOrderSync
    {
        private readonly OrderRepository _orderRepository;
        private readonly CustomerClient _customerClient;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly AcumaticaOrderPull _acumaticaOrderPull;

        public AcumaticaOrderSync(
                OrderRepository orderRepository, 
                CustomerClient customerClient, 
                SalesOrderClient salesOrderClient, 
                AcumaticaOrderPull acumaticaOrderPull)
        {
            _orderRepository = orderRepository;
            _customerClient = customerClient;
            _salesOrderClient = salesOrderClient;
            _acumaticaOrderPull = acumaticaOrderPull;
        }


        public void Run()
        {
            var shopifyOrders = 
                    _orderRepository.RetrieveShopifyOrdersNotSynced();

            foreach (var shopifyOrder in shopifyOrders)
            {
                // TODO - Make this useful state information conveniently available
                // ... via the OrderSyncStatusService
                //
                if (!shopifyOrder.LineItemsAreReadyToSync())
                {
                    continue;
                }

                if (!shopifyOrder.IsPaid())
                {
                    continue;
                }

                SyncOrderWithAcumatica(shopifyOrder);
            }
        }

        public void RunByShopifyId(long shopifyOrderId)
        {
            var shopifyOrder = _orderRepository.RetrieveShopifyOrder(shopifyOrderId);
            SyncOrderWithAcumatica(shopifyOrder);
        }

        private void SyncOrderWithAcumatica(UsrShopifyOrder shopifyOrderRecord)
        {
            if (!shopifyOrderRecord.UsrAcumaticaSalesOrders.Any())
            {
                PushOrderToAcumatica(shopifyOrderRecord);
            }

            _orderRepository.Refresh(shopifyOrderRecord);
        }


        private void PushOrderToAcumatica(UsrShopifyOrder shopifyOrderRecord)
        {
            var customer = SyncCustomer(shopifyOrderRecord);

            var shopifyOrder
                = shopifyOrderRecord
                    .ShopifyJson
                    .DeserializeToOrder();

            var salesOrder = new SalesOrder();

            // TODO - convert this to a constant or configurable item
            salesOrder.OrderType = "SO".ToValue();
            salesOrder.Description = $"Shopify Order #{shopifyOrder.order_number}".ToValue();
            salesOrder.CustomerID = customer.AcumaticaCustomerId.ToValue();
            salesOrder.TaxTotal = ((double) shopifyOrder.total_tax).ToValue();
            salesOrder.Details = new List<SalesOrderDetail>();
            

            foreach (var lineItem in shopifyOrderRecord.UsrShopifyOrderLineItems)
            {
                var stockItem
                    = lineItem
                        .UsrShopifyVariant
                        .UsrAcumaticaStockItems
                        .First();

                var shopifyLineItem
                    = shopifyOrder
                        .line_items
                        .First(x => x.id == lineItem.ShopifyLineItemId);

                var salesOrderDetail = new SalesOrderDetail();

                salesOrderDetail.InventoryID = stockItem.ItemId.ToValue();

                salesOrderDetail.OrderQty 
                    = ((double)shopifyLineItem.quantity).ToValue();

                salesOrderDetail.ExtendedPrice =
                    ((double) shopifyLineItem.TotalAfterDiscount).ToValue();

                salesOrder.Details.Add(salesOrderDetail);
            }

            var resultJson 
                = _salesOrderClient.AddSalesOrder(salesOrder.SerializeToJson());

            var resultSalesOrder 
                = resultJson.DeserializeFromJson<SalesOrder>();

            var acumaticaRecord
                = _acumaticaOrderPull.UpsertOrderToPersist(resultSalesOrder);

            acumaticaRecord.UsrShopifyOrder = shopifyOrderRecord;
            _orderRepository.SaveChanges();
        }

        

        public UsrAcumaticaCustomer 
                SyncCustomer(UsrShopifyOrder shopifyOrder)
        {
            var customer = shopifyOrder.UsrShopifyCustomer;
            UsrAcumaticaCustomer output;

            if (!customer.UsrAcumaticaCustomers.Any())
            {
                output = PushCustomer(customer);
            }
            else
            {
                output = customer.UsrAcumaticaCustomers.FirstOrDefault();
            }

            return output;
        }
        

        private UsrAcumaticaCustomer 
                PushCustomer(UsrShopifyCustomer shopifyCustomerRecord)
        {
            var shopifyCustomer =
                shopifyCustomerRecord
                    .ShopifyJson
                    .DeserializeFromJson<Push.Shopify.Api.Customer.Customer>();
            
            var name = shopifyCustomer.first_name + " " + shopifyCustomer.last_name;
            var shopifyAddress = shopifyCustomer.default_address;

            var customer = new Customer();
            customer.CustomerName = name.ToValue();
            
            var address = new Address();
            address.AddressLine1 = shopifyAddress.address1.ToValue();
            address.AddressLine2 = shopifyAddress.address2.ToValue();
            address.City = shopifyAddress.city.ToValue();
            address.State = shopifyAddress.province.ToValue();
            address.PostalCode = shopifyAddress.zip.ToValue();

            var mainContact = new Contact();
            mainContact.Address = address;
            mainContact.FirstName = shopifyCustomer.first_name.ToValue();
            mainContact.LastName = shopifyCustomer.last_name.ToValue();
            mainContact.Phone1 = shopifyCustomer.phone.ToValue();
            mainContact.Email = shopifyCustomer.email.ToValue();

            customer.MainContact = mainContact;

            var resultJson 
                = _customerClient.AddNewCustomer(customer.SerializeToJson());

            var newAcumaticaCustomer = resultJson.DeserializeFromJson<Customer>();
            var acumaticaMonsterRecord = newAcumaticaCustomer.ToMonsterRecord();
            acumaticaMonsterRecord.UsrShopifyCustomer = shopifyCustomerRecord;

            _orderRepository.InsertAcumaticaCustomer(acumaticaMonsterRecord);
            return acumaticaMonsterRecord;
        }
        
    }
}

