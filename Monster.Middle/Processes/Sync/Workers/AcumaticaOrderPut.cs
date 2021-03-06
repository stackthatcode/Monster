﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Http;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Analysis;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.PendingActions;
using Monster.Middle.Processes.Sync.Model.TaxTranfser;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Sync.Workers
{
    public class AcumaticaOrderPut
    {
        private readonly SettingsRepository _settingsRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly AcumaticaCustomerPut _acumaticaCustomerSync;
        private readonly AcumaticaOrderPaymentPut _acumaticaOrderPaymentPut;
        private readonly AcumaticaTimeZoneService _acumaticaTimeZoneService;
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;
        private readonly AcumaticaHttpContext _acumaticaHttpContext;
        private readonly ShopifyJsonService _shopifyJsonService;
        private readonly PendingActionService _pendingActionService;
        private readonly JobMonitoringService _jobMonitoringService;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly ExecutionLogService _logService;
        private readonly IPushLogger _systemLogger;


        public AcumaticaOrderPut(
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
                SalesOrderClient salesOrderClient,
                AcumaticaOrderRepository acumaticaOrderRepository,
                AcumaticaCustomerPut acumaticaCustomerSync, 
                AcumaticaOrderPaymentPut acumaticaOrderPaymentPut,
                PendingActionService pendingActionService,
                JobMonitoringService jobMonitoringService,
                AcumaticaTimeZoneService acumaticaTimeZoneService,
                AcumaticaHttpContext acumaticaHttpContext,
                ShopifyJsonService shopifyJsonService,
                SettingsRepository settingsRepository,
                ExecutionLogService logRepository,
                IPushLogger systemLogger)
        {
            _syncOrderRepository = syncOrderRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _salesOrderClient = salesOrderClient;
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _acumaticaCustomerSync = acumaticaCustomerSync;
            _acumaticaOrderPaymentPut = acumaticaOrderPaymentPut;
            _acumaticaTimeZoneService = acumaticaTimeZoneService;
            _pendingActionService = pendingActionService;
            _jobMonitoringService = jobMonitoringService;
            _shopifyJsonService = shopifyJsonService;
            _settingsRepository = settingsRepository;
            _logService = logRepository;
            _systemLogger = systemLogger;
            _acumaticaHttpContext = acumaticaHttpContext;
        }


        public ConcurrentQueue<long> BuildOrderPutQueue()
        {
            var output = new ConcurrentQueue<long>();
            var orders = _syncOrderRepository.RetrieveShopifyOrdersToPut();

            foreach (var order in orders)
            {
                output.Enqueue(order);
            }

            return output;
        }

        public void RunQueue(ConcurrentQueue<long> queue)
        {
            while (true)
            {
                long shopifyOrderId;

                if (_jobMonitoringService.DetectCurrentJobInterrupt())
                {
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
                CorrectSalesOrderWithUnknownRef(shopifyOrderId);

                // *** SAVE THIS, JONES! - This little branch of logic increases throughput!!
                //
                var orderPreAction = _pendingActionService.Create(shopifyOrderId).OrderAction;
                if (orderPreAction.ActionCode == ActionCode.UpdateInAcumatica && !orderPreAction.IsValid)
                {
                    _acumaticaOrderPaymentPut.ProcessOrder(shopifyOrderId);
                }

                var orderAction = _pendingActionService.Create(shopifyOrderId).OrderAction;
                if(!orderAction.IsValid)
                {
                    _logService.Log(LogBuilder.SkippingInvalidShopifyOrder(shopifyOrderId));
                    return;
                }

                if (orderAction.ActionCode == ActionCode.CreateInAcumatica)
                {
                    CreateSalesOrder(shopifyOrderId);
                    _acumaticaOrderPaymentPut.ProcessOrder(shopifyOrderId);
                    return;
                }

                if (orderAction.ActionCode == ActionCode.UpdateInAcumatica)
                {
                    _acumaticaOrderPaymentPut.ProcessOrder(shopifyOrderId);
                    UpdateExistingSalesOrder(shopifyOrderId);
                    return;
                }

                if (orderAction.ActionCode == ActionCode.CreateBlankSyncRecord)
                {
                    CreateBlankSalesOrderRecord(shopifyOrderId);
                    return;
                }
            }
            catch (Exception ex)
            {
                _systemLogger.Error(ex);
                _logService.Log($"Encountered error syncing Shopify Order {shopifyOrderId}");
                _syncOrderRepository.IncreaseOrderErrorCount(shopifyOrderId);
            }
        }


        private void CorrectSalesOrderWithUnknownRef(long shopifyOrderId)
        {
            var shopifyOrderRecord = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);

            if (!shopifyOrderRecord.HasSyncWithUnknownNbr())
            {
                return;
            }

            var salesOrderRecord = shopifyOrderRecord.AcumaticaSalesOrder;

            var customerOrderRef = shopifyOrderRecord.ShopifyOrderId.ToString();
            var findOrders = _salesOrderClient.FindSalesOrder(customerOrderRef);
            if (findOrders.Count == 0)
            {
                _logService.Log(LogBuilder.ClearingUnknownAcumaticaSalesOrderRef(shopifyOrderRecord));
                _acumaticaOrderRepository.DeleteSalesOrder(shopifyOrderRecord.AcumaticaSalesOrder);
                return;
            }

            // Heuristic for now
            //
            var salesOrder = findOrders.OrderBy(x => x.OrderNbr.value).First();
            salesOrderRecord.Ingest(salesOrder);
            _logService.Log(LogBuilder.FillingUnknownAcumaticaSalesOrderRef(shopifyOrderRecord, salesOrderRecord));
            _acumaticaOrderRepository.SaveChanges();
        }


        // Push Order
        //
        private void CreateSalesOrder(long shopifyOrderId)
        {
            var shopifyOrderRecord = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);
            var acumaticaCustomer = PushNonExistentCustomer(shopifyOrderRecord);

            var logContent = LogBuilder.CreateAcumaticaSalesOrder(shopifyOrderRecord);
            _logService.Log(logContent);

            // Write the Sales Order to Acumatica
            //
            var salesOrder = BuildNewSalesOrder(shopifyOrderRecord, acumaticaCustomer);

            // Create the Sync record *first*
            //
            var newRecord = new AcumaticaSalesOrder();
            newRecord.ShopifyOrderMonsterId = shopifyOrderRecord.MonsterId;
            newRecord.ShopifyCustomerMonsterId = acumaticaCustomer.ShopifyCustomerMonsterId;
            newRecord.DateCreated = DateTime.UtcNow;
            newRecord.LastUpdated = DateTime.UtcNow;
            _acumaticaOrderRepository.InsertSalesOrder(newRecord);

            // Write to Acumatica Sales Order API
            //
            var resultJson = _salesOrderClient.WriteSalesOrder(salesOrder.SerializeToJson(), Expand.Totals);
            var newOrder = resultJson.ToSalesOrderObj();
            newRecord.Ingest(newOrder);

            if (!newRecord.AcumaticaIsTaxValid)
            {
                shopifyOrderRecord.NeedsOrderPut = true;
            }
            else
            {
                shopifyOrderRecord.NeedsOrderPut = false;
            }

            shopifyOrderRecord.ErrorCount = 0;
            _syncOrderRepository.SaveChanges();
        }

        public void UpdateExistingSalesOrder(long shopifyOrderId)
        {
            var shopifyOrderRecord = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);
            var acumaticaRecord = shopifyOrderRecord.SyncedSalesOrder();

            if (acumaticaRecord.AcumaticaOrderNbr == AcumaticaSyncConstants.BlankRefNbr)
            {
                shopifyOrderRecord.NeedsOrderPut = false;
                shopifyOrderRecord.ErrorCount = 0;
                _syncOrderRepository.SaveChanges();
                return;
            }

            var logContent = LogBuilder.UpdatingAcumaticaSalesOrder(shopifyOrderRecord);
            _logService.Log(logContent);

            var updateOrderJson = BuildSalesOrderUpdate(shopifyOrderRecord).SerializeToJson();
            var resultJson = _salesOrderClient.WriteSalesOrder(updateOrderJson, Expand.Totals);
            var salesOrder = resultJson.ToSalesOrderObj();

            acumaticaRecord.AcumaticaIsTaxValid = salesOrder.IsTaxValid.value;
            acumaticaRecord.AcumaticaStatus = salesOrder.Status.value;
            acumaticaRecord.AcumaticaFreight = (decimal)salesOrder.Totals.Freight.value;
            acumaticaRecord.AcumaticaLineTotal = (decimal)salesOrder.Totals.LineTotalAmount.value;
            acumaticaRecord.AcumaticaTaxTotal = (decimal)salesOrder.Totals.TaxTotal.value;
            acumaticaRecord.AcumaticaOrderTotal = (decimal)salesOrder.OrderTotal.value;
            acumaticaRecord.AcumaticaQtyTotal = (int)salesOrder.Details.Sum(x => x.OrderQty.value);
            acumaticaRecord.LastUpdated = DateTime.Now;

            shopifyOrderRecord.NeedsOrderPut = false;
            if (!acumaticaRecord.AcumaticaIsTaxValid)
            {
                shopifyOrderRecord.NeedsOrderPut = true;
            }

            shopifyOrderRecord.ErrorCount = 0;
            _syncOrderRepository.SaveChanges();
        }

        private void CreateBlankSalesOrderRecord(long shopifyOrderId)
        {
            var shopifyRecord = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);

            var acumaticaRecord = new AcumaticaSalesOrder();
            acumaticaRecord.AcumaticaIsTaxValid = true;
            acumaticaRecord.AcumaticaStatus = "N/A";
            acumaticaRecord.AcumaticaOrderNbr = AcumaticaSyncConstants.BlankRefNbr;
            acumaticaRecord.ShopifyOrder = shopifyRecord;
            acumaticaRecord.AcumaticaCustomer = shopifyRecord.ShopifyCustomer.AcumaticaCustomer;
            acumaticaRecord.DateCreated = DateTime.UtcNow;
            acumaticaRecord.LastUpdated = DateTime.UtcNow;

            shopifyRecord.NeedsOrderPut = false;
            _syncOrderRepository.Entities.AcumaticaSalesOrders.Add(acumaticaRecord);
            _syncOrderRepository.SaveChanges();
        }


        // Create new Sales Order
        //
        private SalesOrder BuildNewSalesOrder(ShopifyOrder shopifyOrderRecord, AcumaticaCustomer customer)
        {
            // Get the Shopify Order
            //
            var shopifyOrder = _shopifyJsonService.RetrieveOrder(shopifyOrderRecord.ShopifyOrderId);

            // Header
            //
            var salesOrder = BuildNewSalesOrderHeader(shopifyOrderRecord, shopifyOrder, customer);

            // Detail
            //
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

            if (!shopifyOrder.MaybeShippingRateTitle.IsNullOrEmpty())
            {
                var carrierToShipVia 
                    = _settingsRepository.RetrieveRateToShipVia(shopifyOrder.MaybeShippingRateTitle);

                if (carrierToShipVia != null)
                {
                    salesOrder.ShipVia = carrierToShipVia.AcumaticaShipViaId.ToValue();
                }
            }

            // Payment optimization *** FAIL - PENDING ACUMATICA SUPPORT ***
            //
            //var payment = _acumaticaOrderPaymentPut.BuildPaymentForCreate(shopifyOrderRecord.PaymentTransaction());
            //salesOrder.Payments = new List<object> { payment };

            salesOrder.PaymentRef = shopifyOrderRecord.PaymentTransaction().ShopifyTransactionId.ToString().ToValue();

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
                detail.OrderQty = ((double)lineItem.NetOrderedQuantity).ToValue();
                detail.UnitPrice = ((double) lineItem.price).ToValue();
                detail.DiscountAmount = ((double) lineItem.Discount).ToValue();

                // detail.ExtendedPrice = ((double)lineItem.NetLineAmount).ToValue();

                detail.TaxCategory 
                    = lineItem.taxable
                        ? settings.AcumaticaTaxableCategory.ToValue()
                        : settings.AcumaticaTaxExemptCategory.ToValue();

                output.Add(detail);
            }

            return output;
        }

        private SalesOrder BuildNewSalesOrderHeader(
                ShopifyOrder shopifyOrderRecord, Order shopifyOrder, AcumaticaCustomer customer)
        {
            var transactionRecord = shopifyOrderRecord.PaymentTransaction();
            var payment = _shopifyJsonService.RetrieveTransaction(transactionRecord.ShopifyTransactionId);

            var settings = _settingsRepository.RetrieveSettings();
            var gateway = _settingsRepository.RetrievePaymentGatewayByShopifyId(payment.gateway);

            var salesOrder = new SalesOrder();
            salesOrder.Details = new List<SalesOrderDetail>();

            salesOrder.OrderType = SalesOrderType.SO.ToValue();

            var createdAtUtc = shopifyOrder.created_at.UtcDateTime;
            var acumaticaDate = _acumaticaTimeZoneService.ToAcumaticaTimeZone(createdAtUtc);
            salesOrder.Date = acumaticaDate.Date.ToValue();

            salesOrder.CustomerOrder = $"{shopifyOrder.id}".ToValue();
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
            salesOrder.FreightPrice = ((double)shopifyOrder.NetShippingPrice).ToValue();

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
                Branch = _acumaticaHttpContext.AcumaticaBranch.ToValue()

            };

            var taxTransfer = shopifyOrder.ToSerializedAndZippedTaxTransfer();
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
                output.CountryID = address.country_code.ToValue();
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
            var shopifyOrder = _shopifyJsonService.RetrieveOrder(shopifyOrderRecord.ShopifyOrderId);
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
            salesOrderUpdate.FreightPrice = ((double)shopifyOrder.NetShippingPrice).ToValue();
            salesOrderUpdate.OverrideFreightPrice = true.ToValue();

            foreach (var line_item in shopifyOrder.line_items)
            {
                var variant = 
                    _syncInventoryRepository.RetrieveVariant(line_item.variant_id.Value, line_item.sku);

                var stockItemId = variant.MatchedStockItem().ItemId;
                var salesOrderDetail = existingSalesOrder.DetailByInventoryId(stockItemId);

                var detail = new SalesOrderUpdateDetail();
                detail.id = salesOrderDetail.id;
                detail.OrderQty = ((double)line_item.NetOrderedQuantity).ToValue();
                detail.InventoryID = variant.MatchedStockItem().ItemId.ToValue();

                salesOrderUpdate.Details.Add(detail);
            }

            var taxTransfer = shopifyOrder.ToSerializedAndZippedTaxTransfer();
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

