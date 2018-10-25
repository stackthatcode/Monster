using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Product;


namespace Monster.Middle.Processes.Inventory.Workers
{
    public class AcumaticaInventorySync
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly DistributionClient _inventoryClient;
        private readonly IPushLogger _logger;

        public AcumaticaInventorySync(
                    InventoryRepository inventoryRepository, 
                    BatchStateRepository batchStateRepository, 
                    DistributionClient inventoryClient, 
                    TenantRepository tenantRepository,
                    IPushLogger logger)
        {
            _inventoryRepository = inventoryRepository;
            _batchStateRepository = batchStateRepository;
            _inventoryClient = inventoryClient;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }

        public void BaselineSync()
        {
            var variants = _inventoryRepository.RetrieveShopifyVariants();

            foreach (var variant in variants)
            {
                // Attempt to identify duplicates
                var matchingShopifySkus =
                    _inventoryRepository
                        .RetrieveShopifyVariants(variant.StandardizedSku())
                        .ExcludeMissing()
                        .ExcludeMatched();
                
                if (matchingShopifySkus.Count > 1)
                {
                    _logger.Info("Shopify Variant: " + 
                        $"{variant.ShopifyVariantId}/{variant.ShopifySku} has duplicates");
                    continue;
                }

                // Attempt to Auto-match
                var stockItem =
                    _inventoryRepository
                        .RetreiveAcumaticaStockItem(variant.StandardizedSku());

                if (stockItem != null)
                {
                    if (stockItem.IsMatchedToShopifyVariant())
                    {
                        _logger.Info(
                            $"Acumatica Stock Item {stockItem.ItemId} " +
                            $"is already matched to Shopify Variant " +
                            $"{stockItem.UsrShopifyVariant.ShopifyVariantId}");
                        continue;
                    }
                    else
                    {
                        _logger.Info(
                            $"Auto-matching Stock Item {stockItem.ItemId} " +
                            $"to Shopify Variant {variant.ShopifyVariantId}");

                        stockItem.ShopifyVariantMonsterId = variant.MonsterId;
                        _inventoryRepository.SaveChanges();
                        continue;
                    }
                }

                // With neither duplicates or Auto-matching having succeeded,
                // ... we'll create a new Stock Item in Acumatica
                PushAcumaticaStockItem(variant);
            }
        }

        public void DifferentialSync()
        {

        }

        public void PushAcumaticaStockItem(UsrShopifyVariant variant)
        {
            var preferences = _tenantRepository.RetrievePreferences();
            var defaultItemClass = preferences.AcumaticaDefaultItemClass;
            
            var shopifyVariant
                = variant.ShopifyVariantJson.DeserializeFromJson<Variant>();

            var shopifyProduct
                = variant
                    .UsrShopifyProduct
                    .ShopifyJson
                    .DeserializeFromJson<Product>();

            var newStockItem = new StockItem();
            newStockItem.InventoryID = variant.StandardizedSku().ToValue();
            newStockItem.Description =
                Standards.StockItemTitle(
                    shopifyProduct, shopifyVariant).ToValue();

            newStockItem.ItemClass = defaultItemClass.ToValue();

            var newStockItemJson = newStockItem.SerializeToJson();
            _inventoryClient.AddNewStockItem(newStockItemJson);
        }
    }
}
