using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.PendingActions;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Validation;


namespace Monster.Middle.Processes.Sync.Services
{
    public class OrderValidationService
    {
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly SettingsRepository _settingsRepository;


        public OrderValidationService(
                SyncOrderRepository syncOrderRepository, 
                SettingsRepository settingsRepository, 
                SyncInventoryRepository syncInventoryRepository)
        {
            _syncOrderRepository = syncOrderRepository;
            _settingsRepository = settingsRepository;
            _syncInventoryRepository = syncInventoryRepository;
        }

        public ValidationResult ReadyToCreateOrder(long shopifyOrderId)
        {
            var output = new CreateOrderValidation();
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrderWithNoTracking(shopifyOrderId);
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
                output.HasValidGateway = _settingsRepository.GatewayExistsInConfig(output.ShopifyPaymentGatewayId);
            }

            return output.Result();
        }

        public ValidationResult ReadyToCreateBlankOrder(long shopifyOrderId)
        {
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrderWithNoTracking(shopifyOrderId);
            var validation = new Validation<ShopifyOrder>()
                .Add(x => x.IsEmptyOrCancelled, "This is not an actual empty or cancelled Shopify Order");
            return validation.Run(orderRecord);
        }


        public ValidationResult ReadyToUpdateOrder(long shopifyOrderId)
        {
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrderWithNoTracking(shopifyOrderId);
            var validation = new Validation<ShopifyOrder>()
                .Add(x => x.PutErrorCount < SystemConsts.ErrorThreshold,
                        "Encountered too many errors attempting to synchronize this Order")
                .Add(x => !x.ShopifyTransactions.Any(y => y.NeedsPaymentPut), 
                        "Payments/Refunds need to be synced before updating Sales Order");

            return validation.Run(orderRecord);
        }

        private void BuildLineItemValidations(CreateOrderValidation validation, MonsterSetting settings)
        {
            foreach (var lineItem in validation.ShopifyOrder.line_items)
            {
                if (lineItem.sku == null)
                {
                    validation.LineItemIdsWithUnrecognizedSku.Add(lineItem.id);
                }

                var variant = _syncInventoryRepository.RetrieveLiveVariant(lineItem.sku.StandardizedSku());

                if (variant == null)
                {
                    validation.SkusMissingFromShopify.Add(lineItem.sku);
                    continue;
                }

                if (!variant.IsSynced())
                {
                    validation.SkusNotSyncedInAcumatica.Add(lineItem.sku);
                    continue;
                }

                if (variant.AreSkuAndItemIdMismatched())
                {
                    validation.SkusWithMismatchedStockItemId.Add(lineItem.sku);
                }

                if (variant.AreTaxesMismatched(settings))
                {
                    validation.SkusWithMismatchedTaxes.Add(lineItem.sku);
                }
            }
        }
    }
}
