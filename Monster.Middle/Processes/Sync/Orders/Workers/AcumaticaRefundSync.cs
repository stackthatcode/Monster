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
using Monster.Middle.Processes.Sync.Orders.Model;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Sync.Orders.Workers
{
    public class AcumaticaRefundSync
    {
        private readonly TenantRepository _tenantRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly AcumaticaShipmentPull _acumaticaShipmentPull;
        private readonly SalesOrderClient _salesOrderClient;
        
        public AcumaticaRefundSync(
                    SyncOrderRepository syncOrderRepository,
                    SyncInventoryRepository syncInventoryRepository,
                    AcumaticaShipmentPull acumaticaShipmentPull,
                    SalesOrderClient salesOrderClient, 
                    TenantRepository tenantRepository)
        {
            _syncOrderRepository = syncOrderRepository;
            _acumaticaShipmentPull = acumaticaShipmentPull;
            _salesOrderClient = salesOrderClient;
            _tenantRepository = tenantRepository;
            _syncInventoryRepository = syncInventoryRepository;
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
            // First write the cancelled items to the Sales Order
            var updatePayload = BuildUpdateForCancellations(refundRecord);
            _salesOrderClient.WriteSalesOrder(updatePayload.SerializeToJson());

            // Second, write Credit Memo to Acumatica for restocked items
            WriteCreditMemoForRestocks(refundRecord);
        }

        public SalesOrderWrite 
                BuildUpdateForCancellations(UsrShopifyRefund refundRecord)
        {
            var shopifyOrderRecord = refundRecord.UsrShopifyOrder;
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();

            var salesOrderRecord = shopifyOrderRecord.MatchingSalesOrder();
            var salesOrder = salesOrderRecord.ToAcuObject();

            var refund =
                shopifyOrder
                    .refunds.First(x => x.id == refundRecord.ShopifyRefundId);

            var salesOrderUpdate = new SalesOrderWrite();
            salesOrderUpdate.OrderType = salesOrder.OrderType.Copy();
            salesOrderUpdate.OrderNbr = salesOrder.OrderNbr.Copy();
            salesOrderUpdate.Hold = false.ToValue();

            // Isolate the corresponding Acumatica Sales Order and Customer
            foreach (var refund_line_item in refund.CancelledLineItems)
            {
                var line_item 
                    = shopifyOrder.LineItem(refund_line_item.line_item_id);

                // TODO *** this needs to be replaced by the Sync Map
                var variant =
                    _syncInventoryRepository
                        .RetrieveVariant(line_item.variant_id, line_item.sku);

                var stockItemId = variant.MatchedStockItem().ItemId;

                var salesOrderDetail = salesOrder.DetailByInventoryId(stockItemId);
                
                var newQuantity =
                    salesOrderDetail.OrderQty.value - refund_line_item.quantity;

                var detail = new SalesOrderUpdateDetail();
                detail.id = salesOrderDetail.id;                
                detail.Quantity = newQuantity.ToValue();

                salesOrderUpdate.Details.Add(detail);
            }

            return salesOrderUpdate;
        }
        
        public bool ValidateCancellationRequest(
                    SalesOrderWrite updatePayload, SalesOrder salesOrder)
        {
            // *** NOTE - please be advised this needs to be tested
            foreach (var updateDetail in updatePayload.Details)
            {
                var orderLineItem = salesOrder.DetailByDetailId(updateDetail.id);

                if (updateDetail.Quantity.value < orderLineItem.QtyOnShipments.value)
                {
                    return false;
                }
            }

            return true;
        }
        
        public void WriteCreditMemoForRestocks(UsrShopifyRefund refundRecord)
        {
            var preferences = _tenantRepository.RetrievePreferences();

            var shopifyOrderRecord = refundRecord.UsrShopifyOrder;
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();

            var salesOrderRecord = shopifyOrderRecord.MatchingSalesOrder();
            var salesOrder = salesOrderRecord.ToAcuObject();

            var refund =
                shopifyOrder.refunds.First(x => x.id == refundRecord.ShopifyRefundId);

            var creditMemo = new CreditMemoWrite();
            creditMemo.OrderType = Constants.CreditMemoType.ToValue();
            creditMemo.CustomerID = salesOrder.CustomerID.Copy();
            
            var taxDetail = new TaxDetails();
            taxDetail.TaxID = preferences.AcumaticaTaxId.ToValue();
            creditMemo.TaxDetails = new List<TaxDetails> { taxDetail };
            
            foreach (var refund_line_item in refund.Returns)
            {
                var line_item
                    = shopifyOrder.LineItem(refund_line_item.line_item_id);

                // TODO *** this needs to be replaced by the Sync Map
                var variant =
                    _syncInventoryRepository
                        .RetrieveVariant(line_item.variant_id, line_item.sku);

                var stockItemId = variant.MatchedStockItem().ItemId;

                var location =
                    _syncInventoryRepository.RetrieveLocation(refund_line_item.location_id.Value);

                var warehouse = location.MatchedWarehouse();


                var detail = new CreditMemoWriteDetail();
                detail.InventoryID = stockItemId.ToValue();
                detail.OrderQty = ((double) refund_line_item.quantity).ToValue();
                detail.WarehouseID = warehouse.AcumaticaWarehouseId.ToValue();
                creditMemo.Details.Add(detail);
            }

            var result 
                = _salesOrderClient.WriteSalesOrder(creditMemo.SerializeToJson());
            var resultCM = result.DeserializeFromJson<SalesOrder>();

            var syncRecord = new UsrShopAcuRefundCm();
            syncRecord.AcumaticaCreditMemoNbr = resultCM.OrderNbr.value;
            syncRecord.UsrShopifyRefund = refundRecord;
            syncRecord.DateCreated = DateTime.UtcNow;
            syncRecord.LastUpdated = DateTime.UtcNow;

            _syncOrderRepository.InsertRefundSync(syncRecord);

            // Write the Sales Tax 
            if (resultCM.TaxDetails.Any())
            {
                var updateCM = new CreditMemoWrite();
                var taxUpdate = new TaxDetails();
                taxUpdate.id = resultCM.TaxDetails.First().id;
                taxUpdate.TaxID = preferences.AcumaticaTaxId.ToValue();
                taxUpdate.TaxRate = ((double) 0).ToValue();
                taxUpdate.TaxableAmount = ((double) 0).ToValue();
                taxUpdate.TaxAmount = ((double) shopifyOrder.total_tax).ToValue();

                var resultJson2
                    = _salesOrderClient.WriteSalesOrder(updateCM.SerializeToJson());
            }
        }
    }
}

