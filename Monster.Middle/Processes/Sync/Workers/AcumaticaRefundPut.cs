using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Persist.Matching;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Workers
{
    public class AcumaticaRefundPut
    {
        private readonly ExecutionLogService _logService;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncRepository;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly AcumaticaOrderGet _acumaticaOrderPull;
        private readonly AcumaticaOrderPut _acumaticaOrderSync;
        private readonly IPushLogger _logger;

        public AcumaticaRefundPut(

                    SyncOrderRepository syncOrderRepository,
                    SyncInventoryRepository syncRepository,
                    SalesOrderClient salesOrderClient, 
                    PreferencesRepository preferencesRepository, 
                    ExecutionLogService logService, 
                    AcumaticaOrderGet acumaticaOrderPull, 
                    AcumaticaOrderPut acumaticaOrderSync,
                    IPushLogger logger)
        {
            _syncOrderRepository = syncOrderRepository;
            _salesOrderClient = salesOrderClient;
            _preferencesRepository = preferencesRepository;
            _logService = logService;
            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaOrderSync = acumaticaOrderSync;
            _syncRepository = syncRepository;
            _logger = logger;
        }


        // Refunds-Cancellations
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
        
        public void PushCancelsQuantityUpdate(ShopifyRefund refundRecord)
        {
            // First, pull down the latest Order in case the Details record id's have mutated
            var salesOrderRecord = refundRecord.ShopifyOrder.MatchingSalesOrder();
            _acumaticaOrderPull.RunAcumaticaOrderDetails(salesOrderRecord.AcumaticaOrderNbr);

            // Push an update to the Sales Order
            var updatePayload = BuildUpdateForCancels(refundRecord);
            var result = _salesOrderClient.WriteSalesOrder(updatePayload.SerializeToJson());

            // Update the local cache of Sales Order
            salesOrderRecord.DetailsJson = result;
            salesOrderRecord.LastUpdated = DateTime.UtcNow;

            // Update the Tax Loaded flag
            refundRecord
                .ShopifyOrder
                .ShopAcuOrderSyncs.First();

            _syncOrderRepository.SaveChanges();
            
            // Update the Sales Taxes
            //_acumaticaOrderSync.PushOrderTaxesToAcumatica(refundRecord.ShopifyOrder);
        }
        
        public SalesOrderUpdateHeader BuildUpdateForCancels(ShopifyRefund refundRecord)
        {
            var shopifyOrderRecord = refundRecord.ShopifyOrder;
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



        // Refunds-Returns
        //
        public void RunReturns()
        {
            var returns = _syncOrderRepository.RetrieveReturnsNotSynced();

            foreach (var _return in returns)
            {
                var status = RefundSyncStatus.Make(_return);

                if (status.IsSyncComplete)
                {
                    continue;
                }


                // *** TODO - this should be a Return for Credit
                //
                // Write a Credit Memo to Acumatica for restocked items
                if (status.DoesNotHaveCreditMemoOrder)
                {
                    var content = LogBuilder.CreateAcumaticaCreditMemo(_return);
                    _logService.Log(content);
                    PushReturnOrder(_return);
                    continue;
                }
            }
        }

        public void PushReturnOrder(ShopifyRefund refundRecord)
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var shopifyOrder = refundRecord.ShopifyOrder.ToShopifyObj();

            var creditMemo = BuildReturnForCredit(refundRecord, preferences);

            var result = _salesOrderClient.WriteSalesOrder(creditMemo.SerializeToJson());
            var resultCmOrder = result.DeserializeFromJson<SalesOrder>();

            var syncRecord = new ShopAcuRefundCm();
            syncRecord.AcumaticaCreditMemoOrderNbr = resultCmOrder.OrderNbr.value;            
            syncRecord.IsCmOrderTaxLoaded = false;
            syncRecord.AcumaticaCreditMemoInvoiceNbr = null;
            syncRecord.IsCmInvoiceReleased = false;
            syncRecord.IsComplete = false;

            syncRecord.ShopifyRefund = refundRecord;
            syncRecord.DateCreated = DateTime.UtcNow;
            syncRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.InsertRefundSync(syncRecord);

            var log =
                $"Created Credit Memo {resultCmOrder.OrderNbr.value} in Acumatica from " +
                $"Shopify Order #{shopifyOrder.order_number}";

            _logService.Log(log);
        }

        private ReturnForCreditWrite 
                    BuildReturnForCredit(ShopifyRefund refundRecord, Preference preferences)
        {
            var shopifyOrderRecord = refundRecord.ShopifyOrder;
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
    }
}

