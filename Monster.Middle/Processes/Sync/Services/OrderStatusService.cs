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
                output.PaymentGateway
                    = _gatewayService.Retrieve(orderRecord.PaymentTransaction().ShopifyGateway);
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

            output.CreateOrderInAcumatica = !orderRecord.ExistsInAcumatica();
            output.OrderSyncValidation = OrderSyncValidation(shopifyOrderId);
            output.UpdateOrderInAcumatica = orderRecord.ExistsInAcumatica() && orderRecord.NeedsOrderPut;

            output.MissingShopifyPayment = !orderRecord.HasPayment();

            if (orderRecord.HasPayment())
            {
                var payment = orderRecord.PaymentTransaction();
                output.ShopifyPaymentAmount = payment.ShopifyAmount;
                output.CreatePaymentInAcumatica = !payment.ExistsInAcumatica();
                output.UpdatePaymentInAcumatica = payment.ExistsInAcumatica();
                output.ReleasePaymentInAcumatica 
                    = payment.ExistsInAcumatica() && !payment.AcumaticaPayment.IsReleased;
            }

            foreach (var refundTransactions in orderRecord.RefundTransactions())
            {

            }

            foreach (var creditAdj in orderRecord.CreditAdustmentRefunds())
            {

            }

            foreach (var debitAdj in orderRecord.CreditAdustmentRefunds())
            {

            }
            return output;
        }
    }
}
