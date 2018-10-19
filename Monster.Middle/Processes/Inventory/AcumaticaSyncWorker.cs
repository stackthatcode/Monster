using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
using Monster.Middle.Processes.Inventory.Model;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Inventory
{
    public class AcumaticaSyncWorker
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly InventoryClient _inventoryClient;
        private readonly IPushLogger _logger;

        public AcumaticaSyncWorker(
                    InventoryRepository inventoryRepository, 
                    BatchStateRepository batchStateRepository, 
                    InventoryClient inventoryClient, 
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
            var preferences = _tenantRepository.RetrievePreferences();
            var defaultItemClass = preferences.AcumaticaDefaultItemClass;

            var unmatchedVariants 
                = _inventoryRepository
                        .RetrieveUnmatchedShopifyVariants();

            foreach (var variant in unmatchedVariants)
            {
                var matchingShopifySkus =
                    _inventoryRepository
                        .RetrieveShopifyVariants(variant.ShopifySku);
                        
                if (matchingShopifySkus.Count > 1)
                {
                    variant.Status = VariantStatus.ExceptionDuplicate;
                    _inventoryRepository.SaveChanges();
                    break;
                }

                var standarizedSku = variant.StandardizedSku();

                var stockItem =
                    _inventoryRepository
                        .RetreiveAcumaticaUnmatchedStockItem(standarizedSku);

                if (stockItem != null)
                {
                    _inventoryRepository.InsertMatch(
                        variant.MonsterId, stockItem.MonsterId);
                    break;
                }

                // Create Stock Item in Acumatica

                var shopifyVariant 
                    = variant.ShopifyJson.DeserializeFromJson<Variant>();

                var shopifyProduct 
                    = variant
                        .UsrShopifyProduct
                        .ShopifyJson
                        .DeserializeFromJson<Product>();

                var newStockItem = new StockItem();
                newStockItem.InventoryID = standarizedSku.ToValue();
                newStockItem.Description =
                    Standardize.StockItemTitle(
                            shopifyProduct, shopifyVariant).ToValue();

                newStockItem.ItemClass = defaultItemClass.ToValue();
                
                var newStockItemJson = newStockItem.SerializeToJson();
                _inventoryClient.AddNewStockItem(newStockItemJson);
            }
        }
    }
}
