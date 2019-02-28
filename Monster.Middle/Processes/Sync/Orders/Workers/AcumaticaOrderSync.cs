using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Inventory;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Orders.Workers
{
    public class AcumaticaOrderSync
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ExecutionLogRepository _logRepository;
        private readonly IPushLogger _logger;

        private readonly PreferencesRepository _preferencesRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;

        private readonly SalesOrderClient _salesOrderClient;
        private readonly CustomerClient _customerClient;

        private readonly AcumaticaOrderPull _acumaticaOrderPull;
        private readonly AcumaticaCustomerSync _acumaticaCustomerSync;

         
        public AcumaticaOrderSync(
                ILifetimeScope lifetimeScope,
                ExecutionLogRepository logRepository,
                IPushLogger logger,
                PreferencesRepository preferencesRepository,
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
                SalesOrderClient salesOrderClient,
                CustomerClient customerClient,
                AcumaticaOrderPull acumaticaOrderPull, 
                AcumaticaCustomerSync acumaticaCustomerSync)
        {
            _lifetimeScope = lifetimeScope;
            _logRepository = logRepository;
            _logger = logger;

            _preferencesRepository = preferencesRepository;
            _syncOrderRepository = syncOrderRepository;
            _syncInventoryRepository = syncInventoryRepository;

            _salesOrderClient = salesOrderClient;
            _customerClient = customerClient;

            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaCustomerSync = acumaticaCustomerSync;
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
            var preferences = _preferencesRepository.RetrievePreferences();
            var orderStart = preferences.ShopifyOrderNumberStart ?? 0;

            foreach (var order in orders)
            {
                if (order.ShopifyOrderNumber < orderStart)
                {
                    continue;
                }

                if (!order.IsPaid())
                {
                    continue;
                }

                if (!AreLineItemsReadyToSync(order))
                {
                    continue;
                }

                output.Enqueue(order.ShopifyOrderId);
            }

            return output;
        }

        private bool AreLineItemsReadyToSync(UsrShopifyOrder shopifyOrderRecord)
        {
            var order = shopifyOrderRecord.ShopifyJson.DeserializeToOrder();

            foreach (var lineItem in order.line_items)
            {
                if (lineItem.variant_id == null)
                {
                    return false;
                }

                var variant = 
                    _syncInventoryRepository
                        .RetrieveVariant(lineItem.variant_id.Value, lineItem.sku);

                if (variant == null || variant.IsNotMatched())
                {
                    return false;
                }
            }

            return true;
        }
        

        private void PushOrderWithCustomerAndTaxes(long shopifyOrderId)
        {
            var shopifyOrderRecord 
                    = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);

            if (!shopifyOrderRecord.UsrShopAcuOrderSyncs.Any())
            {
                // *** Isolated Unit of Work 
                PushCustomer(shopifyOrderRecord);
                PushOrder(shopifyOrderRecord);
                PushOrderTaxes(shopifyOrderRecord);
            }
        }
        


        // Push Order
        //
        private void PushOrder(UsrShopifyOrder shopifyOrderRecord)
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var shopifyOrder = shopifyOrderRecord.ShopifyJson.DeserializeToOrder();
            var shopifyCustomer = _syncOrderRepository.RetrieveCustomer(shopifyOrder.customer.id);
            var customer = shopifyCustomer.Match();

            // Sales Order Header
            var salesOrder = BuildNewSalesOrderHeader(shopifyOrder, customer, preferences);

            // Shipping Contact
            var shippingContact = BuildShippingContact(shopifyOrder);
            salesOrder.ShipToContactOverride = true.ToValue();
            salesOrder.ShipToContact = shippingContact;

            // Shipping Address
            var shippingAddress = BuildShippingAddress(shopifyOrder);
            salesOrder.ShipToAddressOverride = true.ToValue();
            salesOrder.ShipToAddress = shippingAddress;
            
            foreach (var lineItem in shopifyOrder.line_items)
            {
                var variant =
                    _syncInventoryRepository
                        .RetrieveVariant(lineItem.variant_id.Value, lineItem.sku);

                var stockItem = variant.MatchedStockItem();
                
                var detail = new SalesOrderDetail();

                detail.InventoryID = stockItem.ItemId.ToValue();
                detail.OrderQty = ((double)lineItem.RefundCancelAdjustedQuantity).ToValue();
                detail.ExtendedPrice = ((double) lineItem.TotalAfterDiscount).ToValue();                
                detail.TaxCategory = preferences.AcumaticaTaxCategory.ToValue();

                salesOrder.Details.Add(detail);
            }
            
            // *** IMPORTANT - do not use the member that contains 
            var resultJson = _salesOrderClient.WriteSalesOrder(salesOrder.SerializeToJson());
            var resultSalesOrder = resultJson.DeserializeFromJson<SalesOrder>();

            // Record the Sales Order Record to SQL

            var acumaticaRecord = _acumaticaOrderPull.UpsertOrderToPersist(resultSalesOrder);
            var taxDetailsId = resultSalesOrder.TaxDetails.First().id;            
            _syncOrderRepository
                    .InsertOrderSync(shopifyOrderRecord, acumaticaRecord, taxDetailsId, false);

            _logRepository.InsertExecutionLog(
                    $"Created Order {acumaticaRecord.AcumaticaOrderNbr} in Acumatica " +
                    $"from Shopify Order #{shopifyOrderRecord.ShopifyOrderNumber}");
        }


        private static SalesOrder BuildNewSalesOrderHeader(
                Order shopifyOrder, UsrAcumaticaCustomer customer, UsrPreference preferences)
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
            

            // Create Tax Details payload
            var taxDetails = new TaxDetails();
            taxDetails.TaxID = preferences.AcumaticaTaxId.ToValue();
            //taxDetails.TaxRate
            //    = ((double)(shopifyOrder.total_tax / shopifyOrder.TaxableAmountTotalAfterRefundCancels)).ToValue();
            taxDetails.TaxableAmount = ((double)shopifyOrder.TaxableAmountTotalAfterRefundCancels).ToValue();
            taxDetails.TaxAmount = ((double)shopifyOrder.TaxTotalAfterRefundCancels).ToValue();

            salesOrder.TaxDetails = new List<TaxDetails> { taxDetails };
            salesOrder.TaxTotal = ((double)shopifyOrder.total_tax).ToValue();
            salesOrder.IsTaxValid = true.ToValue();
            
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
                shippingAddress.PostalCode = shopifyOrder.shipping_address.province_code.ToValue();
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



        // Push Order Taxes
        //
        public void PushOrderTaxes(UsrShopifyOrder shopifyOrderRecord)
        {
            // Arrange
            var syncRecord = shopifyOrderRecord.UsrShopAcuOrderSyncs.FirstOrDefault();
            if (syncRecord == null)
            {
                return;
            }

            var preferences = _preferencesRepository.RetrievePreferences();
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();
            var acumaticaRecord = syncRecord.UsrAcumaticaSalesOrder;
            
            // Create Tax Details payload
            var taxDetails = new TaxDetails();
            taxDetails.id = syncRecord.AcumaticaTaxDetailId;
            taxDetails.TaxID = preferences.AcumaticaTaxId.ToValue();
            //taxDetails.TaxRate 
            //    = ((double)(shopifyOrder.total_tax / shopifyOrder.TaxableAmountTotalAfterRefundCancels)).ToValue();
            taxDetails.TaxableAmount = ((double)shopifyOrder.TaxableAmountTotalAfterRefundCancels).ToValue();
            taxDetails.TaxAmount = ((double)shopifyOrder.TaxTotalAfterRefundCancels).ToValue();

            // Create Sales Order payload
            var orderUpdate = new SalesOrderTaxUpdate();
            orderUpdate.OrderNbr = acumaticaRecord.AcumaticaOrderNbr.ToValue();
            orderUpdate.TaxDetails = new List<TaxDetails> { taxDetails };

            // PUT to Acumatica
            var result = _salesOrderClient.WriteSalesOrder(orderUpdate.SerializeToJson());

            // Update the Sync Record
            acumaticaRecord.DetailsJson = result;
            syncRecord.IsTaxLoadedToAcumatica = true;
            syncRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.Entities.SaveChanges();

            _logRepository.InsertExecutionLog(
                $"Wrote Taxes for Order {acumaticaRecord.AcumaticaOrderNbr} in Acumatica " +
                $"from Shopify Order #{shopifyOrderRecord.ShopifyOrderNumber}");
        }
        

        // Push Customer
        //
        public UsrAcumaticaCustomer PushCustomer(UsrShopifyOrder shopifyOrder)
        {
            var customer =
                _syncOrderRepository
                    .RetrieveCustomer(shopifyOrder.UsrShopifyCustomer.ShopifyCustomerId);

            if (!customer.HasMatch())
            {
                return _acumaticaCustomerSync.PushCustomer(customer);
            }
            else
            {
                return customer.Match();
            }
        }
    }
}

