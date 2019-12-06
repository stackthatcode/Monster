using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.TaxTransfer;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Newtonsoft.Json;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Transactions;


namespace Monster.Middle.Processes.Sync.Workers
{
    public class AcumaticaOrderPut
    {
        private readonly ExecutionLogService _logService;
        private readonly SettingsRepository _settingsRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly AcumaticaOrderGet _acumaticaOrderPull;
        private readonly AcumaticaCustomerPut _acumaticaCustomerSync;
        private readonly AcumaticaOrderPaymentPut _acumaticaOrderPaymentPut;
        private readonly OrderSyncValidationService _orderStatusService;
        private readonly JobMonitoringService _jobMonitoringService;
        private readonly IPushLogger _systemLogger;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;


        public AcumaticaOrderPut(
                ExecutionLogService logRepository,
                SettingsRepository settingsRepository,
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
                SalesOrderClient salesOrderClient,
                AcumaticaOrderRepository acumaticaOrderRepository,
                AcumaticaCustomerPut acumaticaCustomerSync, 
                AcumaticaOrderPaymentPut acumaticaOrderPaymentPut,
                OrderSyncValidationService orderStatusService,
                JobMonitoringService jobMonitoringService,
                IPushLogger systemLogger)
        {
            _logService = logRepository;
            _settingsRepository = settingsRepository;
            _syncOrderRepository = syncOrderRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _salesOrderClient = salesOrderClient;
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _acumaticaCustomerSync = acumaticaCustomerSync;
            _acumaticaOrderPaymentPut = acumaticaOrderPaymentPut;
            _orderStatusService = orderStatusService;
            _jobMonitoringService = jobMonitoringService;
            _systemLogger = systemLogger;
        }


        public ConcurrentQueue<long> BuildOrderPutQueue()
        {
            var output = new ConcurrentQueue<long>();
            var orders = _syncOrderRepository.RetrieveShopifyOrdersToPut();

            foreach (var order in orders)
            {
                output.Enqueue(order.ShopifyOrderId);
            }

            return output;
        }

        public void RunNonParallel()
        {
            var queue = BuildOrderPutQueue();
            RunQueue(queue);
        }
        
        public void RunQueue(ConcurrentQueue<long> queue)
        {
            while (true)
            {
                long shopifyOrderId;

                if (_jobMonitoringService.IsInterruptedByJobType(BackgroundJobType.EndToEndSync))
                {
                    _logService.Log(LogBuilder.JobExecutionIsInterrupted());
                    return;
                }

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
        
        public void RunOrder(long shopifyOrderId)
        {
            try
            {
                _systemLogger.Debug($"AcumaticaOrderPut -> RunOrder({shopifyOrderId})");

                var status = _orderStatusService.GetOrderSyncValidator(shopifyOrderId);
                var readyToSyncValidation = status.Result();

                if (!readyToSyncValidation.Success)
                {
                    _syncOrderRepository.UpdateShopifyIsBlocked(shopifyOrderId, true);
                    return;
                }

                _syncOrderRepository.UpdateShopifyIsBlocked(shopifyOrderId, false);

                var orderRecord = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);

                if (!orderRecord.ExistsInAcumatica())
                {
                    var acumaticaCustomer = PushNonExistentCustomer(orderRecord);
                    CreateNewOrder(orderRecord, acumaticaCustomer);

                    _acumaticaOrderPaymentPut.ProcessOrder(orderRecord.ShopifyOrderId);

                }
                else
                {
                    if (_acumaticaOrderPaymentPut.ProcessOrder(orderRecord.ShopifyOrderId))
                    {
                        UpdateExistingOrder(orderRecord);
                    }
                }
            }
            catch(Exception ex)
            {
                _systemLogger.Error(ex);
                _logService.Log($"Encounter error syncing Shopify Order {shopifyOrderId}");
            }
        }


        // Push Order
        //
        private void CreateNewOrder(ShopifyOrder shopifyOrderRecord, AcumaticaCustomer acumaticaCustomer)
        {
            var logContent = LogBuilder.CreateAcumaticaSalesOrder(shopifyOrderRecord);
            _logService.Log(logContent);

            // Write the Sales Order to Acumatica
            //
            var salesOrder = BuilderNewSalesOrder(shopifyOrderRecord, acumaticaCustomer);
            var resultJson = _salesOrderClient.WriteSalesOrder(salesOrder.SerializeToJson());

            // Create the local Order Record and Sync
            //
            var newOrder = resultJson.ToSalesOrderObj();

            var newRecord = new AcumaticaSalesOrder();
            newRecord.ShopifyOrderMonsterId = shopifyOrderRecord.Id;

            // Conspicuously not set - waiting on AcumaticaOrderGet to update this
            //
            //newRecord.AcumaticaShipmentDetailsJson = resultJson;
            //

            newRecord.AcumaticaOrderNbr = newOrder.OrderNbr.value;
            newRecord.AcumaticaStatus = newOrder.Status.value;
            newRecord.AcumaticaIsTaxValid = newOrder.IsTaxValid.value;
            newRecord.ShopifyCustomerMonsterId = acumaticaCustomer.ShopifyCustomerMonsterId;
            newRecord.DateCreated = DateTime.UtcNow;
            newRecord.LastUpdated = DateTime.UtcNow;

            _acumaticaOrderRepository.InsertSalesOrder(newRecord);

            shopifyOrderRecord.NeedsOrderPut = !newRecord.AcumaticaIsTaxValid;
            _syncOrderRepository.SaveChanges();
        }

        public AcumaticaSalesOrder UpdateExistingOrder(ShopifyOrder shopifyOrderRecord)
        {
            var logContent = LogBuilder.UpdatingAcumaticaSalesOrder(shopifyOrderRecord);
            _logService.Log(logContent);

            var updateOrderJson = BuildSalesOrderUpdate(shopifyOrderRecord).SerializeToJson();

            var resultJson = _salesOrderClient.WriteSalesOrder(updateOrderJson);

            var salesOrder = resultJson.ToSalesOrderObj();

            var acumaticaRecord = shopifyOrderRecord.SyncedSalesOrder();
            acumaticaRecord.AcumaticaIsTaxValid = salesOrder.IsTaxValid.value;
            acumaticaRecord.AcumaticaStatus = salesOrder.Status.value;
            acumaticaRecord.LastUpdated = DateTime.Now;
            _syncOrderRepository.SaveChanges();

            shopifyOrderRecord.NeedsOrderPut = !acumaticaRecord.AcumaticaIsTaxValid;
            _syncOrderRepository.SaveChanges();

            return acumaticaRecord;
        }


        // Create new Sales Order
        //
        private SalesOrder BuilderNewSalesOrder(ShopifyOrder shopifyOrderRecord, AcumaticaCustomer customer)
        {
            // Header
            //
            var salesOrder = BuildNewSalesOrderHeader(shopifyOrderRecord, customer);

            // Detail
            //
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();
            salesOrder.Details = BuildSalesOrderDetail(shopifyOrder);

            // Billing Address & Contact
            //
            salesOrder.BillToContactOverride = true.ToValue();
            salesOrder.BillToContact = BuildContact(shopifyOrder, shopifyOrder.billing_address);
            salesOrder.BillToAddressOverride = true.ToValue();
            salesOrder.BillToAddress = BuildAddress(shopifyOrder.billing_address);

            // Shipping Address & Contact
            //
            salesOrder.ShipToContactOverride = true.ToValue();
            salesOrder.ShipToContact = BuildContact(shopifyOrder, shopifyOrder.shipping_address);
            salesOrder.ShipToAddressOverride = true.ToValue();
            salesOrder.ShipToAddress = BuildAddress(shopifyOrder.shipping_address);

            return salesOrder;
        }

        private IList<SalesOrderDetail> BuildSalesOrderDetail(Order shopifyOrder)
        {
            var output = new List<SalesOrderDetail>();
            var settings = _settingsRepository.RetrieveSettings();

            // Build Line Items
            foreach (var lineItem in shopifyOrder.line_items)
            {
                var standardizedSku = Canonizers.StandardizedSku(lineItem.sku);
                var stockItemRecord = _syncInventoryRepository.RetrieveStockItem(standardizedSku);

                var detail = new SalesOrderDetail();
                detail.InventoryID = stockItemRecord.ItemId.ToValue();
                detail.OrderQty = ((double)lineItem.CancelAdjustedQuantity).ToValue();
                detail.UnitPrice = ((double) lineItem.price).ToValue();
                detail.DiscountAmount = ((double) lineItem.Discount).ToValue();
                detail.ExtendedPrice = ((double)lineItem.NetLineAmount).ToValue();

                detail.TaxCategory 
                    = lineItem.taxable
                        ? settings.AcumaticaTaxableCategory.ToValue()
                        : settings.AcumaticaTaxExemptCategory.ToValue();

                output.Add(detail);
            }

            return output;
        }

        private SalesOrder BuildNewSalesOrderHeader(
                    ShopifyOrder shopifyOrderRecord, AcumaticaCustomer customer)
        {
            var transactionRecord = shopifyOrderRecord.PaymentTransaction();
            var payment = transactionRecord.ShopifyJson.DeserializeFromJson<Transaction>();
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();

            var settings = _settingsRepository.RetrieveSettings();
            var gateway = _settingsRepository.RetrievePaymentGatewayByShopifyId(payment.gateway);

            var salesOrder = new SalesOrder();
            salesOrder.Details = new List<SalesOrderDetail>();

            salesOrder.OrderType = SalesOrderType.SO.ToValue();
            salesOrder.CustomerOrder = shopifyOrder.name.ToValue();
            salesOrder.ExternalRef = $"{shopifyOrder.id}".ToValue();
            salesOrder.Status = SalesOrderStatus.Open.ToValue();
            salesOrder.Hold = false.ToValue();
            salesOrder.Description = $"Shopify Order #{shopifyOrder.order_number}".ToValue();
            salesOrder.CustomerID = customer.AcumaticaCustomerId.ToValue();

            salesOrder.PaymentMethod = gateway.AcumaticaPaymentMethod.ToValue();
            salesOrder.CashAccount = gateway.AcumaticaCashAccount.ToValue();

            // Shipping Settings
            //
            salesOrder.ShippingSettings = new ShippingSettings
            {
                ShipSeparately = true.ToValue(),
                ShippingRule = ShippingRules.BackOrderAllowed.ToValue(),
            };

            // Freight Price and Taxes
            //
            salesOrder.FreightPrice 
                = ((double)shopifyOrder.NetShippingTotal).ToValue();

            salesOrder.OverrideFreightPrice = true.ToValue();
            salesOrder.FreightTaxCategory = shopifyOrder.IsShippingTaxable
                ? settings.AcumaticaTaxableCategory.ToValue()
                : settings.AcumaticaTaxExemptCategory.ToValue();

            // Taxes
            //
            salesOrder.FinancialSettings = new FinancialSettings()
            {
                
                OverrideTaxZone = true.ToValue(),
                CustomerTaxZone = settings.AcumaticaTaxZone.ToValue(),
            };

            var taxTransfer
                = shopifyOrderRecord
                    .ToTaxTransfer()
                    .SerializeToJson(Formatting.None)
                    .ToBase64Zip();

            salesOrder.custom = new SalesOrderUsrTaxSnapshot(taxTransfer);

            return salesOrder;
        }

        private Address BuildAddress(OrderAddress address)
        {
            var output = new Address();
            if (address != null)
            {
                output.AddressLine1 = address.address1.ToValue();
                output.AddressLine2 = address.address2.ToValue();
                output.City = address.city.ToValue();
                output.State = address.province.ToValue();
                output.PostalCode = address.zip.ToValue();
            }
            return output;
        }

        private ContactOverride BuildContact(Order order, OrderAddress address)
        {
            var contactOverride = new ContactOverride();
            contactOverride.Email = order.contact_email.ToValue();

            if (address != null)
            {
                contactOverride.Attention =  address.FullName.ToValue();
                contactOverride.BusinessName = address.company.ToValue();
                contactOverride.Phone1 = address.phone.ToValue();
            }

            return contactOverride;
        }


        // Update existing Sales Order
        //
        public SalesOrderUpdate BuildSalesOrderUpdate(ShopifyOrder shopifyOrderRecord)
        {
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();
            var salesOrderRecord = shopifyOrderRecord.SyncedSalesOrder();

            var existingSalesOrder
                = _salesOrderClient
                    .RetrieveSalesOrder(
                        salesOrderRecord.AcumaticaOrderNbr, SalesOrderType.SO, SalesOrderExpand.Details)
                    .ToSalesOrderObj();
            
            var salesOrderUpdate = new SalesOrderUpdate();
            salesOrderUpdate.OrderType = existingSalesOrder.OrderType.Copy();
            salesOrderUpdate.OrderNbr = existingSalesOrder.OrderNbr.Copy();
            salesOrderUpdate.Hold = false.ToValue();

            // Update the Shipping Cost
            //
            salesOrderUpdate.FreightPrice = ((double)shopifyOrder.NetShippingTotal).ToValue();
            salesOrderUpdate.OverrideFreightPrice = true.ToValue();

            foreach (var line_item in shopifyOrder.line_items)
            {
                var variant = 
                    _syncInventoryRepository.RetrieveVariant(line_item.variant_id.Value, line_item.sku);

                var stockItemId = variant.MatchedStockItem().ItemId;
                var salesOrderDetail = existingSalesOrder.DetailByInventoryId(stockItemId);

                var newQuantity = (double)line_item.CancelAdjustedQuantity;

                var detail = new SalesOrderUpdateDetail();

                detail.id = salesOrderDetail.id;
                detail.OrderQty = newQuantity.ToValue();

                // Not needed - only the row identifier
                //
                detail.InventoryID = variant.MatchedStockItem().ItemId.ToValue();

                salesOrderUpdate.Details.Add(detail);
            }

            var taxTransfer = shopifyOrderRecord
                    .ToTaxTransfer()
                    .SerializeToJson(Formatting.None)
                    .ToBase64Zip();

            salesOrderUpdate.custom = new SalesOrderUsrTaxSnapshot(taxTransfer);
            return salesOrderUpdate;
        }


        // Invoke the Acumatica Customer Sync
        //
        public AcumaticaCustomer PushNonExistentCustomer(ShopifyOrder shopifyOrder)
        {
            var customer = _syncOrderRepository
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

