using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Shopify;
using Monster.Middle.Persist.Multitenant.Sync;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaOrderSync
    {
        private readonly ShopifyInventoryRepository _shopifyInventoryRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;
        private readonly CustomerClient _customerClient;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly AcumaticaOrderPull _acumaticaOrderPull;
        
        public AcumaticaOrderSync(
                    SyncOrderRepository syncOrderRepository,
                    ShopifyInventoryRepository shopifyInventoryRepository,
                    ShopifyOrderRepository orderRepository, 
                    AcumaticaOrderRepository acumaticaOrderRepository, 
                    CustomerClient customerClient,
                    SalesOrderClient salesOrderClient,
                    AcumaticaOrderPull acumaticaOrderPull)
        {
            _syncOrderRepository = syncOrderRepository;
            _shopifyOrderRepository = orderRepository;
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _shopifyInventoryRepository = shopifyInventoryRepository;
            _customerClient = customerClient;
            _salesOrderClient = salesOrderClient;
            _acumaticaOrderPull = acumaticaOrderPull;
        }


        public void Run()
        {
            var shopifyOrders 
                = _syncOrderRepository.RetrieveShopifyOrdersNotSynced();

            foreach (var shopifyOrder in shopifyOrders)
            {
                if (!shopifyOrder.IsPaid())
                {
                    continue;
                }

                if (!AreLineItemsReadyToSync(shopifyOrder))
                {
                    continue;
                }

                SyncOrderWithAcumatica(shopifyOrder);
            }
        }

        public bool AreLineItemsReadyToSync(UsrShopifyOrder shopifyOrderRecord)
        {
            var shopifyOrder
                = shopifyOrderRecord
                    .ShopifyJson
                    .DeserializeToOrder();

            foreach (var lineItem in shopifyOrder.line_items)
            {
                var variant = 
                    _syncOrderRepository
                        .RetrieveVariant(lineItem.variant_id, lineItem.sku);

                if (variant == null || variant.IsNotMatched())
                {
                    return false;
                }
            }

            return true;
        }


        public void RunByShopifyId(long shopifyOrderId)
        {
            var shopifyOrder 
                = _shopifyOrderRepository.RetrieveOrder(shopifyOrderId);

            SyncOrderWithAcumatica(shopifyOrder);
        }

        private void SyncOrderWithAcumatica(UsrShopifyOrder shopifyOrderRecord)
        {
            if (!shopifyOrderRecord.UsrShopAcuOrderSyncs.Any())
            {
                PushOrderToAcumatica(shopifyOrderRecord);
            }
        }


        private void PushOrderToAcumatica(UsrShopifyOrder shopifyOrderRecord)
        {
            var customer = SyncCustomerToAcumatica(shopifyOrderRecord);

            var shopifyOrder
                = shopifyOrderRecord
                    .ShopifyJson
                    .DeserializeToOrder();

            var salesOrder = new SalesOrder();

            // TODO - convert this to a constant or configurable item
            salesOrder.OrderType = "SO".ToValue();
            salesOrder.Status = "Open".ToValue();
            salesOrder.Description = $"Shopify Order #{shopifyOrder.order_number}".ToValue();
            salesOrder.CustomerID = customer.AcumaticaCustomerId.ToValue();
            salesOrder.TaxTotal = ((double) shopifyOrder.total_tax).ToValue();
            salesOrder.Details = new List<SalesOrderDetail>();
            

            foreach (var lineItem in shopifyOrder.line_items)
            {
                var variant =
                    _syncOrderRepository
                        .RetrieveVariant(lineItem.variant_id, lineItem.sku);

                var stockItem = variant.UsrAcumaticaStockItems.First();
                
                var salesOrderDetail = new SalesOrderDetail();

                salesOrderDetail.InventoryID = stockItem.ItemId.ToValue();

                salesOrderDetail.OrderQty 
                    = ((double)lineItem.quantity).ToValue();

                salesOrderDetail.ExtendedPrice =
                    ((double) lineItem.TotalAfterDiscount).ToValue();

                salesOrder.Details.Add(salesOrderDetail);
            }

            var resultJson 
                = _salesOrderClient.AddSalesOrder(salesOrder.SerializeToJson());

            var resultSalesOrder 
                = resultJson.DeserializeFromJson<SalesOrder>();

            var acumaticaRecord
                = _acumaticaOrderPull.UpsertOrderToPersist(resultSalesOrder);

            _syncOrderRepository
                .InsertOrderSync(shopifyOrderRecord, acumaticaRecord);
        }

        

        public UsrAcumaticaCustomer 
                    SyncCustomerToAcumatica(UsrShopifyOrder shopifyOrder)
        {
            var customer =
                _syncOrderRepository.RetrieveCustomer(
                    shopifyOrder.UsrShopifyCustomer.ShopifyCustomerId);

            UsrAcumaticaCustomer output;

            if (!customer.UsrShopAcuCustomerSyncs.Any())
            {
                output = PushCustomer(customer);
            }
            else
            {
                output = customer
                    .UsrShopAcuCustomerSyncs
                    .FirstOrDefault()
                    .UsrAcumaticaCustomer;
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

            // Push to Acumatica API
            var resultJson 
                = _customerClient.AddNewCustomer(customer.SerializeToJson());

            var newAcumaticaCustomer = resultJson.DeserializeFromJson<Customer>();

            var acumaticaMonsterRecord = newAcumaticaCustomer.ToMonsterRecord();

            _acumaticaOrderRepository.InsertCustomer(acumaticaMonsterRecord);

            _syncOrderRepository.InsertCustomerSync(shopifyCustomerRecord, acumaticaMonsterRecord);

            return acumaticaMonsterRecord;
        }
        
    }
}

