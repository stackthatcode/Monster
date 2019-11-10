using System.Collections.Generic;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Services
{
    public class OrderStatusService
    {
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SettingsRepository _settingsRepository;
        private readonly ShopifyPaymentGatewayService _gatewayService;
        private readonly ShopifyUrlService _urlService;

        public OrderStatusService(
                SyncInventoryRepository inventoryRepository, 
                SyncOrderRepository orderRepository, 
                SettingsRepository settingsRepository, 
                ShopifyPaymentGatewayService gatewayService, 
                ShopifyUrlService urlService)
        {
            _syncInventoryRepository = inventoryRepository;
            _syncOrderRepository = orderRepository;
            _settingsRepository = settingsRepository;
            _gatewayService = gatewayService;
            _urlService = urlService;
        }


        public OrderSyncValidation OrderSyncValidation(long shopifyOrderId)
        {
            var output = new OrderSyncValidation();
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);
            var settings = _settingsRepository.RetrieveSettings();

            // If the Starting Shopify Order weren't populated, we would not be here i.e.
            // ... the Shopify Order would not have been pulled from API
            //
            output.SettingsStartingOrderId = settings.ShopifyOrderId.Value;
            output.ShopifyOrderRecord = orderRecord;
            output.ShopifyOrder = orderRecord.ToShopifyObj();
            output.LineItemsWithUnsyncedVariants = BuildLineItemsWithUnsyncedVariants(output.ShopifyOrder);

            if (orderRecord.HasPayment())
            {
                output.ShopifyPaymentGatewayId = orderRecord.PaymentTransaction().ShopifyGateway;
                output.HasValidGateway = _gatewayService.Exists(output.ShopifyPaymentGatewayId);
            }

            return output;
        }

        private List<LineItem> BuildLineItemsWithUnsyncedVariants(Order shopifyOrder)
        {
            var output = new List<LineItem>();

            foreach (var lineItem in shopifyOrder.line_items)
            {
                if (!lineItem.variant_id.HasValue || lineItem.sku == null)
                {
                    output.Add(lineItem);
                    continue;
                }

                var variant =
                    _syncInventoryRepository.RetrieveVariant(lineItem.variant_id.Value, lineItem.sku);

                if (variant == null || variant.IsNotMatched())
                {
                    output.Add(lineItem);
                    continue;
                }
            }

            return output;
        }

        public OrderPendingActionStatus PendingActionStatus(long shopifyOrderId)
        {
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);
            var order = orderRecord.ToShopifyObj();

            var output = new OrderPendingActionStatus();

            output.ShopifyOrderId = shopifyOrderId;
            output.ShopifyOrderHref = _urlService.ShopifyOrderUrl(shopifyOrderId);
            output.ShopifyOrderName = order.name;

            output.OrderSyncValidation = OrderSyncValidation(shopifyOrderId).IsReadyToSync();

            if (!orderRecord.ExistsInAcumatica())
            {
                output.ShopifyOrderAction = PendingAction.CreateInAcumatica;
            }
            if (orderRecord.ExistsInAcumatica() && orderRecord.NeedsOrderPut)
            {
                output.ShopifyOrderAction = PendingAction.UpdateInAcumatica;
            }

            output.MissingShopifyPayment = !orderRecord.HasPayment();
            if (orderRecord.HasPayment())
            {
                var payment = orderRecord.PaymentTransaction();
                output.ShopifyPaymentAmount = payment.ShopifyAmount;
                output.ShopifyPaymentGateway = payment.ShopifyGateway;

                if (!payment.ExistsInAcumatica())
                {
                    output.ShopifyPaymentAction = PendingAction.CreateInAcumatica;
                }
                if (payment.ExistsInAcumatica() && payment.NeedsPaymentPut)
                {
                    output.ShopifyPaymentAction = PendingAction.UpdateInAcumatica;
                }
                else if (payment.ExistsInAcumatica() && !payment.AcumaticaPayment.IsReleased)
                {
                    output.ShopifyPaymentAction = PendingAction.ReleaseInAcumatica;
                }
            }

            foreach (var refund in orderRecord.RefundTransactions())
            {
                var refundAction = new RefundPendingAction();
                refundAction.RefundAmount = refund.ShopifyAmount;

                if (!refund.ExistsInAcumatica())
                {
                    refundAction.Action = PendingAction.CreateInAcumatica;
                }
                if (refund.ExistsInAcumatica() && !refund.IsReleased())
                {
                    refundAction.Action = PendingAction.ReleaseInAcumatica;
                }

                output.RefundPendingActions.Add(refundAction);
            }

            foreach (var creditAdj in orderRecord.CreditAdustmentRefunds())
            {
                var action = new AdjustmentMemoPendingAction();
                action.Action = PendingAction.CreateInAcumatica;
                action.MemoType = AdjustmentMemoType.CreditMemo;
                action.MemoAmount = creditAdj.CreditAdjustment;

                output.AdjustmentMemoPendingActions.Add(action);
            }

            foreach (var debitAdj in orderRecord.DebitAdustmentRefunds())
            {
                var action = new AdjustmentMemoPendingAction();
                action.Action = PendingAction.CreateInAcumatica;
                action.MemoType = AdjustmentMemoType.DebitMemo;
                action.MemoAmount = debitAdj.DebitAdjustment;

                output.AdjustmentMemoPendingActions.Add(action);
            }

            return output;
        }
    }
}
