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
using Monster.Middle.Processes.Sync.Orders.Model;
using Push.Foundation.Utilities.Json;
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

        public AcumaticaRefundSync(

                    SyncOrderRepository syncOrderRepository,
                    SyncInventoryRepository syncRepository,
                    SalesOrderClient salesOrderClient, 
                    PreferencesRepository preferencesRepository, 
                    ExecutionLogRepository logRepository, 
                    AcumaticaOrderPull acumaticaOrderPull)
        {
            _syncOrderRepository = syncOrderRepository;
            _salesOrderClient = salesOrderClient;
            _preferencesRepository = preferencesRepository;
            _logRepository = logRepository;
            _acumaticaOrderPull = acumaticaOrderPull;
            _syncRepository = syncRepository;
        }


        public void Run()
        {
            var refunds = _syncOrderRepository.RetrieveRefundsNotSynced();

            foreach (var refund in refunds)
            {
                SyncRefund(refund);
            }
        }
        
        
        private void SyncRefund(UsrShopifyRefund refundRecord)
        {
            // First, update the Sales Order based on the cancelled items 
            PushCancellationQuantityChanges(refundRecord);

            // Second, write a Credit Memo to Acumatica for restocked items
            PushCreditMemo(refundRecord);
            PushCreditMemoTaxesForRestocks(refundRecord);
        }
        
        public void PushCancellationQuantityChanges(UsrShopifyRefund refundRecord)
        {
            // First, pull down the latest Order in case the Details record id's have mutated
            var salesOrderRecord = refundRecord.UsrShopifyOrder.MatchingSalesOrder();
            _acumaticaOrderPull.RunAcumaticaOrderDetails(salesOrderRecord.AcumaticaOrderNbr);

            var updatePayload = BuildUpdateForCancellations(refundRecord);
            var result = _salesOrderClient.WriteSalesOrder(updatePayload.SerializeToJson());
            salesOrderRecord.DetailsJson = result;
            _syncOrderRepository.SaveChanges();
        }

        public void PushCreditMemo(UsrShopifyRefund refundRecord)
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var shopifyOrderRecord = refundRecord.UsrShopifyOrder;
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();

            var creditMemo = BuildCreditMemo(refundRecord, preferences);

            var result = _salesOrderClient.WriteSalesOrder(creditMemo.SerializeToJson());
            var resultCmOrder = result.DeserializeFromJson<SalesOrder>();

            var syncRecord = new UsrShopAcuRefundCm();
            syncRecord.AcumaticaCreditMemoNbr = resultCmOrder.OrderNbr.value;
            syncRecord.AcumaticaTaxDetailId = resultCmOrder.TaxDetails.First().id;
            syncRecord.IsTaxLoadedToAcumatica = false;
            syncRecord.UsrShopifyRefund = refundRecord;
            syncRecord.DateCreated = DateTime.UtcNow;
            syncRecord.LastUpdated = DateTime.UtcNow;
            _syncOrderRepository.InsertRefundSync(syncRecord);

            var log =
                $"Created Refund {resultCmOrder.OrderNbr.value} in Acumatica from " +
                $"(Order #{shopifyOrder.order_number})";

            _logRepository.InsertExecutionLog(log);
        }

        public void PushCreditMemoTaxesForRestocks(UsrShopifyRefund refundRecord)
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var shopifyOrderRecord = refundRecord.UsrShopifyOrder;
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();
            
            var syncRecord = refundRecord.UsrShopAcuRefundCms.First();
            
            // Write the Sales Tax 
            var taxUpdate = new TaxDetails();
            taxUpdate.id = syncRecord.AcumaticaTaxDetailId;
            taxUpdate.TaxID = preferences.AcumaticaTaxId.ToValue();
            taxUpdate.TaxRate = ((double)0).ToValue();
            taxUpdate.TaxableAmount = ((double)shopifyOrder.TaxableAmountTotal).ToValue();
            taxUpdate.TaxAmount = ((double)shopifyOrder.total_tax).ToValue();

            var creditMemoWrite = new CreditMemoWrite();
            creditMemoWrite.OrderNbr = syncRecord.AcumaticaCreditMemoNbr.ToValue();
            creditMemoWrite.TaxDetails = new List<TaxDetails>() {taxUpdate};

            var result = _salesOrderClient.WriteSalesOrder(creditMemoWrite.SerializeToJson());

            var log = $"Wrote taxes for Refund {syncRecord.AcumaticaCreditMemoNbr} in Acumatica";
            _logRepository.InsertExecutionLog(log);

            syncRecord.IsTaxLoadedToAcumatica = true;
            syncRecord.LastUpdated = DateTime.Today;
            _syncOrderRepository.SaveChanges();
        }



        public SalesOrderUpdateHeader BuildUpdateForCancellations(UsrShopifyRefund refundRecord)
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

        private CreditMemoWrite BuildCreditMemo(UsrShopifyRefund refundRecord, UsrPreference preferences)
        {
            var shopifyOrderRecord = refundRecord.UsrShopifyOrder;
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();

            var salesOrderRecord = shopifyOrderRecord.MatchingSalesOrder();
            var salesOrder = salesOrderRecord.ToAcuObject();

            var refund = shopifyOrder.refunds.First(x => x.id == refundRecord.ShopifyRefundId);

            var creditMemo = new CreditMemoWrite();
            creditMemo.OrderType = AcumaticaConstants.CreditMemoType.ToValue();
            creditMemo.CustomerID = salesOrder.CustomerID.Copy();
            creditMemo.Description = $"Shopify Order #{shopifyOrder.order_number} Refund {refund.id}".ToValue();

            var taxDetail = new TaxDetails();
            taxDetail.TaxID = preferences.AcumaticaTaxId.ToValue();
            creditMemo.TaxDetails = new List<TaxDetails> {taxDetail};

            foreach (var _return in refund.Returns)
            {
                var detail = BuildReturnDetail(shopifyOrder, _return);
                creditMemo.Details.Add(detail);
            }

            return creditMemo;
        }

        private CreditMemoWriteDetail BuildReturnDetail(Order shopifyOrder, RefundLineItem _return)
        {
            var lineItem = shopifyOrder.LineItem(_return.line_item_id);
            var variant = _syncRepository.RetrieveVariant(lineItem.variant_id.Value, lineItem.sku);

            var stockItemId = variant.MatchedStockItem().ItemId;
            var location = _syncRepository.RetrieveLocation(_return.location_id.Value);
            var warehouse = location.MatchedWarehouse();

            var detail = new CreditMemoWriteDetail();
            detail.InventoryID = stockItemId.ToValue();
            detail.OrderQty = ((double) _return.quantity).ToValue();
            detail.WarehouseID = warehouse.AcumaticaWarehouseId.ToValue();
            return detail;
        }
    }
}

