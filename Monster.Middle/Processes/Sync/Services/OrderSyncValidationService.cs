using Monster.Middle.Misc.Shopify;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;


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

            BuildLineItemValidations(output, settings);

            if (orderRecord.HasPayment())
            {
                output.ShopifyPaymentGatewayId = orderRecord.PaymentTransaction().ShopifyGateway;
                output.HasValidGateway = _gatewayService.Exists(output.ShopifyPaymentGatewayId);
            }

            return output;
        }

        private void BuildLineItemValidations(OrderSyncValidation validation, MonsterSetting settings)
        {
            foreach (var lineItem in validation.ShopifyOrder.line_items)
            {
                if (!lineItem.variant_id.HasValue || lineItem.sku == null)
                {
                    continue;
                }

                var variant = _syncInventoryRepository.RetrieveVariant(lineItem.variant_id.Value, lineItem.sku);

                if (variant == null)
                {
                    validation.LineItemIdsWithUnrecognizedVariants.Add(lineItem.id);
                    continue;
                }

                if (variant.IsNotMatched())
                {
                    validation.SkusNotSyncedInAcumatica.Add(variant.ShopifySku);
                }

                if (variant.AreSkuAndItemIdMismatched())
                {
                    validation.SkusWithMismatchedStockItemId.Add(variant.ShopifySku);
                }

                if (variant.AreTaxesMismatched(settings))
                {
                    validation.SkusWithMismatchedTaxes.Add(variant.ShopifySku);
                }
            }
        }
    }
}
