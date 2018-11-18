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
using Monster.Middle.Processes.Orders.Workers.Model;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaOrderSync
    {
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly CustomerClient _customerClient;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly AcumaticaOrderPull _acumaticaOrderPull;
        

        public AcumaticaOrderSync(
                    SyncOrderRepository syncOrderRepository,
                    SyncInventoryRepository syncInventoryRepository,
                    AcumaticaOrderRepository acumaticaOrderRepository, 
                    CustomerClient customerClient,
                    SalesOrderClient salesOrderClient,
                    AcumaticaOrderPull acumaticaOrderPull)
        {
            _syncOrderRepository = syncOrderRepository;
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _customerClient = customerClient;
            _salesOrderClient = salesOrderClient;
            _acumaticaOrderPull = acumaticaOrderPull;
            _syncInventoryRepository = syncInventoryRepository;
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
                    _syncInventoryRepository
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
                = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);

            SyncOrderWithAcumatica(shopifyOrder);
        }

        private void SyncOrderWithAcumatica(UsrShopifyOrder shopifyOrderRecord)
        {
            if (!shopifyOrderRecord.UsrShopAcuOrderSyncs.Any())
            {
                PushOrderToAcumatica(shopifyOrderRecord);
            }
        }


        // Assumes that the 
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
            salesOrder.Hold = false.ToValue();
            salesOrder.Description = $"Shopify Order #{shopifyOrder.order_number}".ToValue();
            salesOrder.CustomerID = customer.AcumaticaCustomerId.ToValue();
            salesOrder.TaxTotal = ((double) shopifyOrder.total_tax).ToValue();
            salesOrder.Details = new List<SalesOrderDetail>();
            

            foreach (var lineItem in shopifyOrder.line_items)
            {
                var variant =
                    _syncInventoryRepository
                        .RetrieveVariant(lineItem.variant_id, lineItem.sku);

                var stockItem = variant.MatchedStockItem();
                
                var salesOrderDetail = new SalesOrderDetail();

                salesOrderDetail.InventoryID = stockItem.ItemId.ToValue();

                salesOrderDetail.OrderQty 
                    = ((double)lineItem.RefundCancelAdjustedQuantity).ToValue();

                salesOrderDetail.ExtendedPrice =
                    ((double) lineItem.TotalAfterDiscount).ToValue();

                salesOrder.Details.Add(salesOrderDetail);
            }

            var resultJson 
                = _salesOrderClient.WriteSalesOrder(salesOrder.SerializeToJson());

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

            // Push new Customer to Acumatica API
            var newCustomerJson 
                = _customerClient.AddNewCustomer(customer.SerializeToJson());
            var newCustomer = newCustomerJson.DeserializeFromJson<Customer>();

            // Create record in Monster for Customer
            var acumaticaMonsterRecord = newCustomer.ToMonsterRecord();
            _acumaticaOrderRepository.InsertCustomer(acumaticaMonsterRecord);

            // Create a sync record
            _syncOrderRepository
                .InsertCustomerSync(shopifyCustomerRecord, acumaticaMonsterRecord);

            return acumaticaMonsterRecord;
        }
        
    }
}

