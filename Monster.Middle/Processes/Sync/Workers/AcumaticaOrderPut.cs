using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Distribution;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.TaxTransfer;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Persist.Matching;
using Monster.Middle.Processes.Sync.Status;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Sync.Workers
{
    public class AcumaticaOrderPut
    {
        private readonly ExecutionLogService _logService;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly AcumaticaOrderGet _acumaticaOrderPull;
        private readonly AcumaticaCustomerPut _acumaticaCustomerSync;
        private readonly OrderStatusService _orderStatusService;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;


        public AcumaticaOrderPut(
                ExecutionLogService logRepository,
                PreferencesRepository preferencesRepository,
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
                SalesOrderClient salesOrderClient,
                AcumaticaOrderRepository acumaticaOrderRepository,
                AcumaticaCustomerPut acumaticaCustomerSync, 
                OrderStatusService orderStatusService)
        {
            _logService = logRepository;
            _preferencesRepository = preferencesRepository;
            _syncOrderRepository = syncOrderRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _salesOrderClient = salesOrderClient;
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _acumaticaCustomerSync = acumaticaCustomerSync;
            _orderStatusService = orderStatusService;
        }

        
        public void RunNonParallel()
        {
            var queue = BuildOrderPutQueue();
            RunWorker(queue);
        }
        
        public void RunWorker(ConcurrentQueue<long> queue)
        {
            while (true)
            {
                long shopifyOrderId;

                if (queue.TryDequeue(out shopifyOrderId))
                {
                    RunOrder(shopifyOrderId);
                }
                else
                {
                    break;
                }
            }
        }
        
        
        public ConcurrentQueue<long> BuildOrderPutQueue()
        {
            var output = new ConcurrentQueue<long>();
            var orders = _syncOrderRepository.RetrieveShopifyOrdersNotSynced();
            
            foreach (var order in orders)
            {
                var status = _orderStatusService.ShopifyOrderStatus(order.ShopifyOrderId);

                if (!status.IsReadyToSync().Success)
                {
                    continue;
                }

                output.Enqueue(order.ShopifyOrderId);
            }

            return output;
        }
        public void RunOrder(long shopifyOrderId)
        {
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);

            var acumaticaCustomer = PushNonExistentCustomer(orderRecord);
            PushOrder(orderRecord, acumaticaCustomer);
        }
        

        // Push Order
        //
        private void PushOrder(ShopifyOrder shopifyOrderRecord, AcumaticaCustomer acumaticaCustomer)
        {
            if (shopifyOrderRecord.HasMatch())
            {
                var logContent = LogBuilder.CreateAcumaticaSalesOrder(shopifyOrderRecord);
                _logService.Log(logContent);

                CreateNewOrder(shopifyOrderRecord, acumaticaCustomer);
            }
            else
            {
                var logContent = LogBuilder.CreateAcumaticaSalesOrder(shopifyOrderRecord);
                _logService.Log(logContent);

                UpdateExistingOrder(shopifyOrderRecord);
            }
        }

        private void UpdateExistingOrder(ShopifyOrder shopifyOrderRecord)
        {
            throw new NotImplementedException();
        }

        private void CreateNewOrder(ShopifyOrder shopifyOrderRecord, AcumaticaCustomer acumaticaCustomer)
        {
            var shopifyOrder = shopifyOrderRecord.ShopifyJson.DeserializeToOrder();
            shopifyOrderRecord.HasMatch();

            var salesOrder = BuilderSalesOrder(shopifyOrder, acumaticaCustomer);
            var resultJson = _salesOrderClient.WriteSalesOrder(salesOrder.SerializeToJson());

            var orderRecord = _acumaticaOrderRepository.RetrieveSalesOrder(salesOrder.OrderNbr.value);

            if (orderRecord == null)
            {
                var newOrder = resultJson.DeserializeFromJson<SalesOrder>();
                var newRecord = new AcumaticaSalesOrder();

                newRecord.AcumaticaOrderNbr = newOrder.OrderNbr.value;
                newRecord.DetailsJson = resultJson;
                newRecord.ShipmentsJson = null;
                newRecord.AcumaticaStatus = newOrder.Status.value;
                newRecord.CustomerMonsterId = acumaticaCustomer.Id;
                newRecord.DateCreated = DateTime.UtcNow;
                newRecord.LastUpdated = DateTime.UtcNow;

                _acumaticaOrderRepository.InsertSalesOrder(newRecord);
                _syncOrderRepository.InsertOrderSync(shopifyOrderRecord, newRecord);

                newRecord.Sync().NeedsAcumaticaPut = false;
                _syncOrderRepository.SaveChanges();

            }
            else
            {
                orderRecord.DetailsJson = salesOrder.SerializeToJson();
                orderRecord.LastUpdated = DateTime.Now;

                if (orderRecord.HasMatch())
                {
                    orderRecord.Sync().NeedsAcumaticaPut = false;
                }
                _syncOrderRepository.SaveChanges();
            }
        }



        private SalesOrder BuilderSalesOrder(Order shopifyOrder, AcumaticaCustomer customer)
        {
            // Header
            //
            var salesOrder = BuildNewSalesOrderHeader(shopifyOrder, customer);

            // Detail
            //
            salesOrder.Details = BuildSalesOrderDetail(shopifyOrder);

            // Shipping Contact
            //
            salesOrder.ShipToContactOverride = true.ToValue();
            salesOrder.ShipToContact = BuildShippingContact(shopifyOrder);

            // Shipping Address
            //
            salesOrder.ShipToAddressOverride = true.ToValue();
            salesOrder.ShipToAddress = BuildShippingAddress(shopifyOrder);

            return salesOrder;
        }

        private IList<SalesOrderDetail> BuildSalesOrderDetail(Order shopifyOrder)
        {
            var output = new List<SalesOrderDetail>();

            // Build Line Items
            foreach (var lineItem in shopifyOrder.line_items)
            {
                var variant =
                    _syncInventoryRepository
                        .RetrieveVariant(lineItem.variant_id.Value, lineItem.sku);

                var stockItemRecord = variant.MatchedStockItem();
                var stockItem = stockItemRecord.AcumaticaJson.DeserializeFromJson<StockItem>();

                var detail = new SalesOrderDetail();

                detail.InventoryID = stockItemRecord.ItemId.ToValue();
                detail.OrderQty = ((double)lineItem.RefundCancelAdjustedQuantity).ToValue();
                detail.ExtendedPrice = ((double)lineItem.PriceAfterDiscount).ToValue();
                detail.TaxCategory = stockItem.TaxCategory.value.ToValue();

                output.Add(detail);
            }

            return output;
        }

        private SalesOrder 
                BuildNewSalesOrderHeader(Order shopifyOrder, AcumaticaCustomer customer)
        {
            var preferences = _preferencesRepository.RetrievePreferences();

            var salesOrder = new SalesOrder();
            salesOrder.Details = new List<SalesOrderDetail>();

            salesOrder.OrderType = SalesOrderType.SO.ToValue();
            salesOrder.Status = "Open".ToValue();
            salesOrder.Hold = false.ToValue();
            salesOrder.ExternalRef = $"{shopifyOrder.order_number}".ToValue();
            salesOrder.Description = $"Shopify Order #{shopifyOrder.order_number}".ToValue();
            salesOrder.CustomerID = customer.AcumaticaCustomerId.ToValue();
            salesOrder.PaymentMethod = preferences.AcumaticaPaymentMethod.ToValue();
            salesOrder.CashAccount = preferences.AcumaticaPaymentCashAccount.ToValue();
            
            salesOrder.FinancialSettings = new FinancialSettings()
            {
                OverrideTaxZone = true.ToValue(),
                CustomerTaxZone = preferences.AcumaticaTaxZone.ToValue(),
            };

            var taxTransferJson = shopifyOrder.ToTaxTransfer().SerializeToJson();

            salesOrder.custom = new SalesOrderCustom()
            {
                Document = new SalesOrderCustom.CustomDocument()
                {
                    UsrTaxSnapshot = new SalesOrderCustom.CustomField
                    {
                        type = "CustomStringField",
                        value = taxTransferJson,
                    }
                }
            };

            // Shipping Settings
            salesOrder.ShippingSettings = new ShippingSettings
            {
                ShipSeparately = true.ToValue(),
                ShippingRule = "Back Order Allowed".ToValue(),
            };

            return salesOrder;
        }

        private Address BuildShippingAddress(Order shopifyOrder)
        {
            var shippingAddress = new Address();
            if (shopifyOrder.shipping_address != null)
            {
                shippingAddress.AddressLine1 = shopifyOrder.shipping_address.address1.ToValue();
                shippingAddress.AddressLine2 = shopifyOrder.shipping_address.address2.ToValue();
                shippingAddress.City = shopifyOrder.shipping_address.city.ToValue();
                shippingAddress.State = shopifyOrder.shipping_address.province.ToValue();
                shippingAddress.PostalCode = shopifyOrder.shipping_address.zip.ToValue();
            }
            return shippingAddress;
        }

        private ContactOverride BuildShippingContact(Order shopifyOrder)
        {
            var shippingContact = new ContactOverride();
            shippingContact.Email = shopifyOrder.contact_email.ToValue();

            if (shopifyOrder.shipping_address != null)
            {
                shippingContact.Attention = shopifyOrder.shipping_address.FullName.ToValue();
                shippingContact.BusinessName = shopifyOrder.shipping_address.company.ToValue();
                shippingContact.Phone1 = shopifyOrder.shipping_address.phone.ToValue();
            }

            return shippingContact;
        }


        // Invoke the Acumatica Customer Sync
        //
        public AcumaticaCustomer PushNonExistentCustomer(ShopifyOrder shopifyOrder)
        {
            var customer =
                _syncOrderRepository
                    .RetrieveCustomer(shopifyOrder.ShopifyCustomer.ShopifyCustomerId);

            if (customer.HasMatch())
            {
                return customer.Match();
            }
            else
            { 
                return _acumaticaCustomerSync.PushCustomer(customer);
            }
        }
    }
}

