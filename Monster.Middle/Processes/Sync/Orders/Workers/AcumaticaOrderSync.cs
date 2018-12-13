﻿using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Orders.Model;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Orders.Workers
{
    public class AcumaticaOrderSync
    {
        private readonly TenantRepository _tenantRepository;
        private readonly JobRepository _jobRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly AcumaticaOrderPull _acumaticaOrderPull;
        private readonly AcumaticaCustomerSync _acumaticaCustomerSync;


        public AcumaticaOrderSync(
                    TenantRepository tenantRepository,
                    JobRepository jobRepository,
                    SyncOrderRepository syncOrderRepository,
                    SyncInventoryRepository syncInventoryRepository,
                    SalesOrderClient salesOrderClient,
                    AcumaticaOrderPull acumaticaOrderPull, 
                    AcumaticaCustomerSync acumaticaCustomerSync)
        {
            _syncOrderRepository = syncOrderRepository;
            _salesOrderClient = salesOrderClient;
            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaCustomerSync = acumaticaCustomerSync;
            _tenantRepository = tenantRepository;
            _jobRepository = jobRepository;
            _syncInventoryRepository = syncInventoryRepository;
        }


        public void Run()
        {
            var shopifyOrders 
                = _syncOrderRepository.RetrieveShopifyOrdersNotSynced();
            var preferences = _tenantRepository.RetrievePreferences();
            var orderStart = preferences.ShopifyOrderPushStart ?? 1000;

            foreach (var shopifyOrder in shopifyOrders)
            {
                if (shopifyOrder.ShopifyOrderNumber < orderStart)
                {
                    continue;
                }

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
                PushOrderTaxesToAcumatica(shopifyOrderRecord);
            }
        }

        
        private void PushOrderToAcumatica(UsrShopifyOrder shopifyOrderRecord)
        {
            var preferences = _tenantRepository.RetrievePreferences();

            var customer = SyncCustomerToAcumatica(shopifyOrderRecord);

            var shopifyOrder
                = shopifyOrderRecord
                    .ShopifyJson
                    .DeserializeToOrder();

            var salesOrder = new SalesOrder();
            salesOrder.Details = new List<SalesOrderDetail>();
            salesOrder.OrderType = Constants.SalesOrderType.ToValue();
            salesOrder.Status = "Open".ToValue();
            salesOrder.Hold = false.ToValue();
            salesOrder.Description = $"Shopify Order #{shopifyOrder.order_number}".ToValue();
            salesOrder.CustomerID = customer.AcumaticaCustomerId.ToValue();
            salesOrder.PaymentMethod = preferences.AcumaticaPaymentMethod.ToValue();
            salesOrder.CashAccount = preferences.AcumaticaPaymentCashAccount.ToValue();

            salesOrder.ShippingSettings = new ShippingSettings
            {
                ShipSeparately = true.ToValue(),
                ShippingRule = "Back Order Allowed".ToValue(),
            };
            
            foreach (var lineItem in shopifyOrder.line_items)
            {
                var variant =
                    _syncInventoryRepository
                        .RetrieveVariant(lineItem.variant_id.Value, lineItem.sku);

                var stockItem = variant.MatchedStockItem();
                
                var salesOrderDetail = new SalesOrderDetail();

                salesOrderDetail.InventoryID = stockItem.ItemId.ToValue();

                salesOrderDetail.OrderQty 
                    = ((double)lineItem.RefundCancelAdjustedQuantity).ToValue();

                salesOrderDetail.ExtendedPrice =
                    ((double) lineItem.TotalAfterDiscount).ToValue();
                
                salesOrderDetail.TaxCategory 
                        = preferences.AcumaticaTaxCategory.ToValue();

                salesOrder.Details.Add(salesOrderDetail);
            }
            
            salesOrder.FinancialSettings = new FinancialSettings()
            {
                OverrideTaxZone = true.ToValue(),
                CustomerTaxZone = preferences.AcumaticaTaxZone.ToValue(),
            };

            var taxDetail = new TaxDetails();
            taxDetail.TaxID = preferences.AcumaticaTaxId.ToValue();
            salesOrder.TaxDetails = new List<TaxDetails> { taxDetail };


            // Record the Sales Order Record to SQL
            var resultJson
                = _salesOrderClient
                    .WriteSalesOrder(salesOrder.SerializeToJson());

            var resultSalesOrder = resultJson.DeserializeFromJson<SalesOrder>();
            var acumaticaRecord
                = _acumaticaOrderPull.UpsertOrderToPersist(resultSalesOrder);
            
            var taxDetailsId = resultSalesOrder.TaxDetails.First().id;
            
            _syncOrderRepository
                .InsertOrderSync(
                    shopifyOrderRecord, acumaticaRecord, taxDetailsId, false);

            _jobRepository.InsertExecutionLog(
                $"Created Order {acumaticaRecord.AcumaticaOrderNbr} in Acumatica " +
                $"from Shopify Order #{shopifyOrderRecord.ShopifyOrderNumber}");
        }

        private void PushOrderTaxesToAcumatica(UsrShopifyOrder shopifyOrderRecord)
        {
            // Arrange
            var syncRecord = shopifyOrderRecord.UsrShopAcuOrderSyncs.FirstOrDefault();
            if (syncRecord == null)
            {
                return;
            }
            var preferences = _tenantRepository.RetrievePreferences();
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();
            var acumaticaRecord = syncRecord.UsrAcumaticaSalesOrder;
            
            // Create Tax Details payload
            var taxUpdate = new TaxDetails();
            taxUpdate.id = syncRecord.AcumaticaTaxDetailId;
            taxUpdate.TaxID = preferences.AcumaticaTaxId.ToValue();
            taxUpdate.TaxRate = ((double)0).ToValue();
            taxUpdate.TaxableAmount = ((double)shopifyOrder.TaxableAmountTotal).ToValue();
            taxUpdate.TaxAmount = ((double)shopifyOrder.total_tax).ToValue();

            // Create Sales Order payload
            var orderUpdate = new SalesOrderWrite();
            orderUpdate.OrderNbr = acumaticaRecord.AcumaticaOrderNbr.ToValue();
            orderUpdate.TaxDetails = new List<TaxDetails> { taxUpdate };
            var result = 
                _salesOrderClient.WriteSalesOrder(orderUpdate.SerializeToJson());

            // Update the Sync Record
            acumaticaRecord.DetailsJson = result;
            syncRecord.IsTaxLoadedToAcumatica = true;
            syncRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.Entities.SaveChanges();

            _jobRepository.InsertExecutionLog(
                $"Wrote Taxes for Order {acumaticaRecord.AcumaticaOrderNbr} in Acumatica " +
                $"from Shopify Order #{shopifyOrderRecord.ShopifyOrderNumber}");
        }


        public UsrAcumaticaCustomer
                SyncCustomerToAcumatica(UsrShopifyOrder shopifyOrder)
        {
            var customer =
                _syncOrderRepository.RetrieveCustomer(
                    shopifyOrder.UsrShopifyCustomer.ShopifyCustomerId);

            var output = _acumaticaCustomerSync.PushCustomer(customer);
            return output;
        }

    }
}

