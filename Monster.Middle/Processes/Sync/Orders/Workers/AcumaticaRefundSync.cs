using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AcumaticaRefundSync
    {
        private readonly ExecutionLogRepository _logRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncRepository;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly AcumaticaOrderPull _acumaticaOrderPull;
        private readonly AcumaticaOrderSync _acumaticaOrderSync;
        private readonly IPushLogger _logger;

        public AcumaticaRefundSync(

                    SyncOrderRepository syncOrderRepository,
                    SyncInventoryRepository syncRepository,
                    SalesOrderClient salesOrderClient, 
                    PreferencesRepository preferencesRepository, 
                    ExecutionLogRepository logRepository, 
                    AcumaticaOrderPull acumaticaOrderPull, 
                    AcumaticaOrderSync acumaticaOrderSync,
                    IPushLogger logger)
        {
            _syncOrderRepository = syncOrderRepository;
            _salesOrderClient = salesOrderClient;
            _preferencesRepository = preferencesRepository;
            _logRepository = logRepository;
            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaOrderSync = acumaticaOrderSync;
            _syncRepository = syncRepository;
            _logger = logger;
        }


        // Refunds + Cancellations
        //
        public void RunCancels()
        {
            var cancels = _syncOrderRepository.RetrieveCancelsNotSynced();
            _logger.Info("Shopify Cancels on hold for now ***");

            //foreach (var cancel in cancels)
            //{
            //    PushCancelsQuantityUpdate(cancel);
            //    PushCancelsTaxesUpdate(cancel);
            //}
        }
        
        public void PushCancelsQuantityUpdate(UsrShopifyRefund refundRecord)
        {
            // First, pull down the latest Order in case the Details record id's have mutated
            var salesOrderRecord = refundRecord.UsrShopifyOrder.MatchingSalesOrder();
            _acumaticaOrderPull.RunAcumaticaOrderDetails(salesOrderRecord.AcumaticaOrderNbr);

            // Push an update to the Sales Order
            var updatePayload = BuildUpdateForCancels(refundRecord);
            var result = _salesOrderClient.WriteSalesOrder(updatePayload.SerializeToJson());

            // Update the local cache of Sales Order
            salesOrderRecord.DetailsJson = result;
            salesOrderRecord.LastUpdated = DateTime.UtcNow;

            // Update the Tax Loaded flag
            refundRecord
                .UsrShopifyOrder
                .UsrShopAcuOrderSyncs.First()
                .IsTaxLoadedToAcumatica = false;

            _syncOrderRepository.SaveChanges();
            
            // Update the Sales Taxes
            //_acumaticaOrderSync.PushOrderTaxesToAcumatica(refundRecord.UsrShopifyOrder);
        }

        public SalesOrderUpdateHeader BuildUpdateForCancels(UsrShopifyRefund refundRecord)
        {
            var shopifyOrderRecord = refundRecord.UsrShopifyOrder;
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();

            var salesOrderRecord = shopifyOrderRecord.MatchingSalesOrder();
            var salesOrder = salesOrderRecord.ToAcuObject();
            var refund = shopifyOrder.refunds.First(x => x.id == refundRecord.ShopifyRefundId);

            var salesOrderUpdate = new SalesOrderUpdateHeader();
            salesOrderUpdate.OrderType = salesOrder.OrderType.Copy();
            salesOrderUpdate.OrderNbr = salesOrder.OrderNbr.Copy();
            salesOrderUpdate.Hold = false.ToValue();

            foreach (var cancelledItem in refund.CancelledLineItems)
            {
                var line_item = shopifyOrder.LineItem(cancelledItem.line_item_id);

                var variant =
                    _syncRepository.RetrieveVariant(line_item.variant_id.Value, line_item.sku);

                var stockItemId = variant.MatchedStockItem().ItemId;
                var salesOrderDetail = salesOrder.DetailByInventoryId(stockItemId);

                var newQuantity = (double)line_item.RefundCancelAdjustedQuantity;

                var detail = new SalesOrderUpdateDetail();
                detail.id = salesOrderDetail.id;
                //detail.InventoryID = variant.MatchedStockItem().ItemId.ToValue();
                detail.Quantity = newQuantity.ToValue();

                salesOrderUpdate.Details.Add(detail);
            }

            return salesOrderUpdate;
        }

        // TODO - Abstract and move to AcumaticaOrderPull
        //
        public void PushCancelsTaxesUpdate(UsrShopifyRefund refundRecord)
        {
            // First refresh the SalesOrder -> TaxDetailId
            var salesOrderRecord = refundRecord.UsrShopifyOrder.MatchingSalesOrder();

            var taxId = RetrieveTaxId(salesOrderRecord.AcumaticaOrderNbr, SalesOrderType.SO);

            if (taxId == null)
            {
                refundRecord.IsCancellationSynced = true;
                refundRecord.LastUpdated = DateTime.UtcNow;
                _syncOrderRepository.SaveChanges();
                return;
            }


            salesOrderRecord.UsrShopAcuOrderSyncs.First().AcumaticaTaxDetailId = taxId;
            _syncOrderRepository.SaveChanges();

            _acumaticaOrderSync.RunOrder(refundRecord.ShopifyOrderId);

            // Now we can flag Refunds Cancellation as synced
            refundRecord.IsCancellationSynced = true;
            refundRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.SaveChanges();
        }

        

        // Refunds + Returns
        //
        public void RunReturns()
        {
            var returns = _syncOrderRepository.RetrieveReturnsNotSynced();

            foreach (var _return in returns)
            {
                if (_return.IsSyncComplete())
                {
                    continue;
                }

                // Write a Credit Memo and Taxes (ha!) to Acumatica for restocked items
                if (_return.DoesNotHaveCreditMemoOrder())
                {
                    PushReturnOrder(_return);
                    PushReturnOrderTaxes(_return);
                    continue;
                }

                if (_return.HasCreditMemoOrder() &&
                    _return.CreditMemoTaxesAreNotSynced())
                {
                    PushReturnOrderTaxes(_return);
                    continue;
                }

                if (_return.DoesNotHaveCreditMemoInvoice())
                {
                    DetectReturnInvoice(_return);
                    // No continue - keep processing with altered state
                }

                if (_return.DoesNotHaveCreditMemoInvoice())
                {
                    PrepareReturnInvoice(_return);
                    continue;
                }

                if (_return.HasCreditMemoInvoice() &&
                    _return.DoesNotHaveReleasedInvoice())
                {
                    DetectReleasedInvoice(_return);
                    // No continue - keep processing with altered state
                }

                if (_return.HasCreditMemoInvoice() &&
                    _return.DoesNotHaveReleasedInvoice())
                {
                    ReleaseReturnInvoice(_return);
                    continue;
                }
            }
        }

        public void PushReturnOrder(UsrShopifyRefund refundRecord)
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var shopifyOrder = refundRecord.UsrShopifyOrder.ToShopifyObj();

            var creditMemo = BuildReturnForCredit(refundRecord, preferences);

            var result = _salesOrderClient.WriteSalesOrder(creditMemo.SerializeToJson());
            var resultCmOrder = result.DeserializeFromJson<SalesOrder>();

            var syncRecord = new UsrShopAcuRefundCm();
            syncRecord.AcumaticaCreditMemoOrderNbr = resultCmOrder.OrderNbr.value;            
            syncRecord.IsCmOrderTaxLoaded = false;
            syncRecord.AcumaticaCreditMemoInvoiceNbr = null;
            syncRecord.IsCmInvoiceReleased = false;
            syncRecord.IsComplete = false;

            syncRecord.UsrShopifyRefund = refundRecord;
            syncRecord.DateCreated = DateTime.UtcNow;
            syncRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.InsertRefundSync(syncRecord);

            var log =
                $"Created Credit Memo {resultCmOrder.OrderNbr.value} in Acumatica from " +
                $"Shopify Order #{shopifyOrder.order_number}";

            _logRepository.InsertExecutionLog(log);
        }

        public void PushReturnOrderTaxes(UsrShopifyRefund refundRecord)
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var shopifyOrderRecord = refundRecord.UsrShopifyOrder;
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();            
            var syncRecord = refundRecord.UsrShopAcuRefundCms.First();
            
            // Write the Sales Tax 
            var taxId = RetrieveTaxId(syncRecord.AcumaticaCreditMemoOrderNbr, SalesOrderType.CM);

            if (taxId == null)
            {
                syncRecord.IsCmOrderTaxLoaded = true;
                syncRecord.LastUpdated = DateTime.Today;
                _syncOrderRepository.SaveChanges();
                return;
            }

            var taxUpdate = new TaxDetails();
            taxUpdate.id = taxId;
            taxUpdate.TaxID = preferences.AcumaticaTaxId.ToValue();
            taxUpdate.TaxRate = ((double)0).ToValue();
            taxUpdate.TaxableAmount = ((double)shopifyOrder.TaxableAmountTotalAfterRefundCancels).ToValue();
            taxUpdate.TaxAmount = ((double)shopifyOrder.TaxTotalAfterRefundCancels).ToValue();

            var creditMemoWrite = new ReturnForCreditWrite();
            creditMemoWrite.OrderNbr = syncRecord.AcumaticaCreditMemoOrderNbr.ToValue();
            creditMemoWrite.OrderType = SalesOrderType.CM.ToValue();
            creditMemoWrite.TaxDetails = new List<TaxDetails>() {taxUpdate};

            var result = _salesOrderClient.WriteSalesOrder(creditMemoWrite.SerializeToJson());

            var log = $"Wrote taxes for Credit Memo {syncRecord.AcumaticaCreditMemoOrderNbr} in Acumatica";
            _logRepository.InsertExecutionLog(log);

            syncRecord.IsCmOrderTaxLoaded = true;
            syncRecord.LastUpdated = DateTime.Today;
            _syncOrderRepository.SaveChanges();
        }

        public void DetectReturnInvoice(UsrShopifyRefund refundRecord)
        {
            var syncRecord = refundRecord.UsrShopAcuRefundCms.First();
            var cmOrderNbr = syncRecord.AcumaticaCreditMemoOrderNbr;

            // Get the Sales Order from LIVE
            var json =
                _salesOrderClient.RetrieveSalesOrder(
                    cmOrderNbr, SalesOrderType.CM, SalesOrderExpand.Shipments);
            var salesOrder = json.ToSalesOrderObj();

            if (salesOrder.HasInvoicedShipment()
                && syncRecord.AcumaticaCreditMemoInvoiceNbr == null)
            {
                syncRecord.AcumaticaCreditMemoInvoiceNbr = salesOrder.ShipmentInvoiceNbr();
                syncRecord.LastUpdated = DateTime.UtcNow;
                _syncOrderRepository.SaveChanges();

                var msg = $"Detected CM Invoice {salesOrder.ShipmentInvoiceNbr()} " +
                          $"for CM Order {cmOrderNbr}";
                _logRepository.InsertExecutionLog(msg);
            }
        }
        
        public void PrepareReturnInvoice(UsrShopifyRefund refundRecord)
        {
            var syncRecord = refundRecord.UsrShopAcuRefundCms.First();
            var cmOrderNbr = syncRecord.AcumaticaCreditMemoOrderNbr;

            // Get the Sales Order from LIVE
            var salesOrderPre =
                _salesOrderClient
                    .RetrieveSalesOrder(cmOrderNbr, SalesOrderType.CM, SalesOrderExpand.Shipments)
                    .ToSalesOrderObj();
            
            // Invoke the PrepareInvoice method
            var entityAsString = new {
                entity = new 
                {
                    OrderNbr = cmOrderNbr.ToValue(),
                    OrderType = SalesOrderType.CM.ToValue(),
                }
            }.SerializeToJson();

            _salesOrderClient.PrepareSalesInvoice(entityAsString);

            // Get the Invoice Number and store with the Sync Record
            var salesOrderPost =
                _salesOrderClient
                    .RetrieveSalesOrder(cmOrderNbr, SalesOrderType.CM, SalesOrderExpand.Shipments)
                    .ToSalesOrderObj();

            if (!salesOrderPost.Shipments.Any())
            {
                // Waiting for Acumatica caching to catch-up 
                _logger.Info(
                    "Acumatica Prepare Sales Invoice stale cache detected " +
                    $"for Sales Order Credit Memo {cmOrderNbr}");
                return;
            }

            var invoiceNbr = salesOrderPost.ShipmentInvoiceNbr();
            var log = $"Prepared CM Invoice {invoiceNbr} for CM Order {cmOrderNbr}";
            _logRepository.InsertExecutionLog(log);

            syncRecord.AcumaticaCreditMemoInvoiceNbr = invoiceNbr;
            syncRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.SaveChanges();
        }

        public void DetectReleasedInvoice(UsrShopifyRefund refundRecord)
        {
            var syncRecord = refundRecord.UsrShopAcuRefundCms.First();
            var creditMemoInvoiceNbr = syncRecord.AcumaticaCreditMemoInvoiceNbr;

            // Get the Sales Order from LIVE
            var invoice = 
                _salesOrderClient
                    .RetrieveSalesOrderInvoice(creditMemoInvoiceNbr, SalesInvoiceType.Credit_Memo)
                    .ToSalesOrderInvoiceObj();

            if (invoice.IsReleased() && refundRecord.DoesNotHaveReleasedInvoice())
            {
                syncRecord.IsCmInvoiceReleased = true;
                syncRecord.IsComplete = true;
                syncRecord.LastUpdated = DateTime.UtcNow;
                _syncOrderRepository.SaveChanges();

                var msg = $"Detected Release for CM Invoice {invoice.ReferenceNbr.value}";
                _logRepository.InsertExecutionLog(msg);
            }
        }

        public void ReleaseReturnInvoice(UsrShopifyRefund refundRecord)
        {
            var syncRecord = refundRecord.UsrShopAcuRefundCms.First();
            var creditMemoInvoiceNbr = syncRecord.AcumaticaCreditMemoInvoiceNbr;

            // Get the Sales Order from LIVE
            var invoicePre
                = _salesOrderClient
                    .RetrieveSalesOrderInvoice(creditMemoInvoiceNbr, SalesInvoiceType.Credit_Memo)
                    .ToSalesOrderInvoiceObj();
            
            // Invoke the PrepareInvoice method
            var entityAsString = new { entity = invoicePre }.SerializeToJson();
            _salesOrderClient.ReleaseSalesInvoice(SalesInvoiceType.Credit_Memo, entityAsString);

            // Get the Invoice Number and store with the Sync Record
            var log = $"Released CM Invoice {invoicePre.ReferenceNbr.value}";
            _logRepository.InsertExecutionLog(log);

            syncRecord.IsCmInvoiceReleased = true;
            syncRecord.IsComplete = true;
            syncRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.SaveChanges();
        }



        private ReturnForCreditWrite 
                    BuildReturnForCredit(UsrShopifyRefund refundRecord, UsrPreference preferences)
        {
            var shopifyOrderRecord = refundRecord.UsrShopifyOrder;
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();
            var salesOrderRecord = shopifyOrderRecord.MatchingSalesOrder();
            var salesOrder = salesOrderRecord.ToAcuObject();
            
            var refund = shopifyOrder.refunds.First(x => x.id == refundRecord.ShopifyRefundId);

            var creditMemo = new ReturnForCreditWrite();
            creditMemo.OrderType = SalesOrderType.CM.ToValue();
            creditMemo.CustomerID = salesOrder.CustomerID.Copy();
            creditMemo.Description = $"Shopify Order #{shopifyOrder.order_number} Refund {refund.id}".ToValue();

            var taxDetail = new TaxDetails();
            taxDetail.TaxID = preferences.AcumaticaTaxId.ToValue();
            creditMemo.TaxDetails = new List<TaxDetails> {taxDetail};
            
            creditMemo.FinancialSettings = new FinancialSettings()
            {
                OverrideTaxZone = true.ToValue(),
                CustomerTaxZone = preferences.AcumaticaTaxZone.ToValue(),
            };

            foreach (var _return in refund.Returns)
            {
                var detail = BuildReturnDetail(shopifyOrder, _return);
                creditMemo.Details.Add(detail);
            }

            return creditMemo;
        }

        private ReturnForCreditWriteDetail 
                    BuildReturnDetail(Order shopifyOrder, RefundLineItem _return)
        {
            var lineItem = shopifyOrder.LineItem(_return.line_item_id);
            var variant = _syncRepository.RetrieveVariant(lineItem.variant_id.Value, lineItem.sku);

            // TODO - this could cause failure if there's a change in inventory
            var stockItemId = variant.MatchedStockItem().ItemId;
            var location = _syncRepository.RetrieveLocation(_return.location_id.Value);
            var warehouse = location.MatchedWarehouse();

            var detail = new ReturnForCreditWriteDetail();
            detail.InventoryID = stockItemId.ToValue();
            detail.OrderQty = ((double) _return.quantity).ToValue();
            detail.WarehouseID = warehouse.AcumaticaWarehouseId.ToValue();
            return detail;
        }


        private string RetrieveTaxId(string orderNbr, string orderType)
        {
            var json = 
                _salesOrderClient
                    .RetrieveSalesOrder(orderNbr, orderType, SalesOrderExpand.TaxDetails);

            var order = json.DeserializeFromJson<SalesOrder>();
            return order.TaxDetails.Any() ? order.TaxDetails.First().id : null;
        }
        
    }
}

