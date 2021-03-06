﻿using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Persist.Matching;
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
        private readonly SettingsRepository _settingsRepository;
        private readonly AcumaticaOrderGet _acumaticaOrderPull;
        private readonly AcumaticaOrderPut _acumaticaOrderSync;
        
        private readonly IPushLogger _logger;

        public AcumaticaRefundPut(

                    SyncOrderRepository syncOrderRepository,
                    SyncInventoryRepository syncRepository,
                    SalesOrderClient salesOrderClient, 
                    SettingsRepository settingsRepository, 
                    ExecutionLogService logService, 
                    AcumaticaOrderGet acumaticaOrderPull, 
                    AcumaticaOrderPut acumaticaOrderSync,
                    IPushLogger logger)
        {
            _syncOrderRepository = syncOrderRepository;
            _salesOrderClient = salesOrderClient;
            _settingsRepository = settingsRepository;
            _logService = logService;
            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaOrderSync = acumaticaOrderSync;
            _syncRepository = syncRepository;
            _logger = logger;
        }


        //private ReturnForCreditWrite BuildReturnForCredit(ShopifyRefund refundRecord, MonsterSetting settings)
        //{
        //    var shopifyOrderRecord = refundRecord.ShopifyOrder;
        //    var shopifyOrder = shopifyOrderRecord.ToShopifyObj();
        //    var salesOrderRecord = shopifyOrderRecord.SyncedSalesOrder();
        //    var salesOrder = salesOrderRecord.ToSalesOrderObj();
            
        //    var refund = shopifyOrder.refunds.First(x => x.id == refundRecord.ShopifyRefundId);

        //    var creditMemo = new ReturnForCreditWrite();
        //    creditMemo.OrderType = SalesOrderType.CM.ToValue();
        //    creditMemo.CustomerID = salesOrder.CustomerID.Copy();
        //    creditMemo.Description = $"Shopify Order #{shopifyOrder.order_number} Refund {refund.id}".ToValue();

        //    creditMemo.FinancialSettings = new FinancialSettings()
        //    {
        //        OverrideTaxZone = true.ToValue(),
        //        CustomerTaxZone = settings.AcumaticaTaxZone.ToValue(),
        //    };

        //    foreach (var _return in refund.Returns)
        //    {
        //        var detail = BuildReturnDetail(shopifyOrder, _return);
        //        creditMemo.Details.Add(detail);
        //    }

        //    return creditMemo;
        //}

        //private ReturnForCreditWriteDetail BuildReturnDetail(Order shopifyOrder, RefundLineItem _return)
        //{
        //    var lineItem = shopifyOrder.LineItem(_return.line_item_id);
        //    var variant = _syncRepository.RetrieveVariant(lineItem.variant_id.Value, lineItem.sku);

        //    // TODO - this could cause failure if there's a change in inventory
        //    var stockItemId = variant.MatchedStockItem().ItemId;
        //    var location = _syncRepository.RetrieveLocation(_return.location_id.Value);
        //    var warehouse = location.MatchedWarehouse();

        //    var detail = new ReturnForCreditWriteDetail();
        //    detail.InventoryID = stockItemId.ToValue();
        //    detail.OrderQty = ((double) _return.quantity).ToValue();
        //    detail.WarehouseID = warehouse.AcumaticaWarehouseId.ToValue();
        //    return detail;
        //}
    }
}

