using System.Collections.Concurrent;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
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
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
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
        private readonly IPushLogger _pushLogger;


        public AcumaticaOrderPut(
                ExecutionLogService logRepository,
                PreferencesRepository preferencesRepository,
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
                SalesOrderClient salesOrderClient,
                AcumaticaOrderGet acumaticaOrderPull, 
                AcumaticaCustomerPut acumaticaCustomerSync, 
                OrderStatusService orderStatusService, 
                IPushLogger pushLogger)
        {
            _logService = logRepository;
            _preferencesRepository = preferencesRepository;
            _syncOrderRepository = syncOrderRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _salesOrderClient = salesOrderClient;
            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaCustomerSync = acumaticaCustomerSync;
            _orderStatusService = orderStatusService;
            _pushLogger = pushLogger;
        }

        
        public void Run()
        {
            var queue = BuildQueue();
            RunWorker(queue);
        }
        
        public void RunWorker(ConcurrentQueue<long> queue)
        {
            while (true)
            {
                long shopifyOrderId;

                if (queue.TryDequeue(out shopifyOrderId))
                {
                    PushOrderWithCustomerAndTaxes(shopifyOrderId);
                }
                else
                {
                    break;
                }
            }
        }
        
        public void RunOrder(long shopifyOrderId)
        {
            PushOrderWithCustomerAndTaxes(shopifyOrderId);
        }
        
        public ConcurrentQueue<long> BuildQueue()
        {
            var output = new ConcurrentQueue<long>();
            var orders = _syncOrderRepository.RetrieveShopifyOrdersNotSynced();
            
            foreach (var order in orders)
            {
                var status = _orderStatusService.ShopifyOrderStatus(order.ShopifyOrderId);

                if (!status.IsReadyToSync().Success)
                {
                    var msg = $"Skipping Sync for Order {order.ShopifyOrderNumber} ({order.ShopifyOrderId}) - " +
                              status.IsReadyToSync().FailureMessages.ToCommaDelimited();
                    _pushLogger.Debug(msg);
                    continue;
                }

                output.Enqueue(order.ShopifyOrderId);
            }

            return output;
        }

        private void PushOrderWithCustomerAndTaxes(long shopifyOrderId)
        {
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);
            RunAcumaticaCustomerSync(orderRecord);
            
            _logService.ExecuteWithFailLog(
                    () => PushOrder(orderRecord),
                    LoggingDescriptors.CreateAcumaticaSalesOrder,
                    LoggingDescriptors.ShopifyOrder(orderRecord));
        }
        


        // Push Order
        //
        private void PushOrder(ShopifyOrder shopifyOrderRecord)
        {
            if (shopifyOrderRecord.HasMatch())
            {
                return;
            }

            var preferences = _preferencesRepository.RetrievePreferences();
            var shopifyOrder = shopifyOrderRecord.ShopifyJson.DeserializeToOrder();
            var shopifyCustomer = _syncOrderRepository.RetrieveCustomer(shopifyOrder.customer.id);
            var customer = shopifyCustomer.Match();

            // Sales Order Header
            var salesOrder = BuildNewSalesOrderHeader(shopifyOrder, customer, preferences);

            // Shipping Contact
            salesOrder.ShipToContactOverride = true.ToValue();
            salesOrder.ShipToContact = BuildShippingContact(shopifyOrder);

            // Shipping Address
            salesOrder.ShipToAddressOverride = true.ToValue();
            salesOrder.ShipToAddress = BuildShippingAddress(shopifyOrder);
            
            // Build Line Items
            foreach (var lineItem in shopifyOrder.line_items)
            {
                var variant =
                    _syncInventoryRepository.RetrieveVariant(lineItem.variant_id.Value, lineItem.sku);

                var stockItem = variant.MatchedStockItem();
                
                var detail = new SalesOrderDetail();

                detail.InventoryID = stockItem.ItemId.ToValue();
                detail.OrderQty = ((double)lineItem.RefundCancelAdjustedQuantity).ToValue();
                detail.ExtendedPrice = ((double) lineItem.PriceAfterDiscount).ToValue();                
                detail.TaxCategory = preferences.AcumaticaTaxCategory.ToValue();

                salesOrder.Details.Add(detail);
            }
            
            // *** IMPORTANT - do not use the member that contains 
            //
            var resultJson = _salesOrderClient.WriteSalesOrder(salesOrder.SerializeToJson());
            var resultSalesOrder = resultJson.DeserializeFromJson<SalesOrder>();

            // Record the Sales Order Record to SQL

            var acumaticaRecord = _acumaticaOrderPull.UpsertOrderToPersist(resultSalesOrder);
            _syncOrderRepository.InsertOrderSync(shopifyOrderRecord, acumaticaRecord);
                
            _logService.InsertExecutionLog(
                    $"Created Order {acumaticaRecord.AcumaticaOrderNbr} in Acumatica " +
                    $"from Shopify Order #{shopifyOrderRecord.ShopifyOrderNumber}");
        }


        private static SalesOrder BuildNewSalesOrderHeader(
                Order shopifyOrder, AcumaticaCustomer customer, Preference preferences)
        {
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

        private static Address BuildShippingAddress(Order shopifyOrder)
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

        private static ContactOverride BuildShippingContact(Order shopifyOrder)
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
        public void RunAcumaticaCustomerSync(ShopifyOrder shopifyOrder)
        {
            var customer =
                _syncOrderRepository.RetrieveCustomer(shopifyOrder.ShopifyCustomer.ShopifyCustomerId);

            if (!customer.HasMatch())
            {
                _acumaticaCustomerSync.PushCustomer(customer);
            }
        }
    }
}

