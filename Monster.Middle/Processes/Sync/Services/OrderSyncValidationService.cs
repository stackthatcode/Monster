using System.Collections.Generic;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Sync.Services
{
    public class OrderSyncValidationService
    {
        private readonly ShopifyPaymentGatewayService _gatewayService;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly SettingsRepository _settingsRepository;


        public OrderSyncValidationService(
                ShopifyPaymentGatewayService gatewayService, 
                SyncOrderRepository syncOrderRepository, 
                SettingsRepository settingsRepository, 
                SyncInventoryRepository syncInventoryRepository)
        {
            _gatewayService = gatewayService;
            _syncOrderRepository = syncOrderRepository;
            _settingsRepository = settingsRepository;
            _syncInventoryRepository = syncInventoryRepository;
        }


        public TransactionSyncValidator 
                GetTransSyncValidator(ShopifyOrder shopifyOrder, ShopifyTransaction thisTransaction)
        {
            return new TransactionSyncValidator
            {
                ValidPaymentGateway = _gatewayService.Exists(thisTransaction.ShopifyGateway),
                ShopifyOrder = shopifyOrder,
                ThisTransaction = thisTransaction,
            };
        }

        public OrderSyncValidation GetOrderSyncValidator(long shopifyOrderId)
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
                    _syncInventoryRepository
                        .RetrieveVariant(lineItem.variant_id.Value, lineItem.sku);

                if (variant == null || variant.IsNotMatched())
                {
                    output.Add(lineItem);
                    continue;
                }
            }

            return output;
        }
    }
}
