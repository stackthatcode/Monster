using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Shopify
{
    public class ShopifyInventoryRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public ShopifyInventoryRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }



        public void InsertLocation(UsrShopifyLocation location)
        {
            Entities.UsrShopifyLocations.Add(location);
            Entities.SaveChanges();
        }

        public IList<UsrShopifyLocation> RetreiveLocations()
        {
            return Entities
                .UsrShopifyLocations
                .Include(x => x.UsrAcumaticaWarehouses)
                .ToList();
        }

        public UsrShopifyLocation RetrieveLocation(long locationId)
        {
            return Entities
                .UsrShopifyLocations
                .Include(x => x.UsrAcumaticaWarehouses)
                .FirstOrDefault(x => x.ShopifyLocationId == locationId);
        }


        // Shopify persistence
        //
        public UsrShopifyProduct 
                RetrieveProduct(long shopifyProductId)
        {
            return Entities
                .UsrShopifyProducts
                .FirstOrDefault(
                        x => x.ShopifyProductId == shopifyProductId);
        }
        
        public UsrShopifyVariant
                RetrieveVariant(long shopifyVariantId, string sku)
        {
            return Entities
                .UsrShopifyVariants
                .Include(x => x.UsrAcumaticaStockItems)
                .FirstOrDefault(
                    x => x.ShopifyVariantId == shopifyVariantId &&
                         x.ShopifySku == sku);
        }

        public DateTime? RetrieveProductMaxUpdatedDate()
        {
            if (Entities.UsrShopifyProducts.Any())
            {
                return Entities.UsrShopifyProducts
                            .Select(x => x.LastUpdated)
                            .Max();
            }
            else
            {
                return (DateTime?) null;
            }
        }
        
        public void InsertProduct(UsrShopifyProduct product)
        {
            Entities.UsrShopifyProducts.Add(product);
            Entities.SaveChanges();
        }

        public void InsertVariant(UsrShopifyVariant variant)
        {
            Entities.UsrShopifyVariants.Add(variant);
            Entities.SaveChanges();
        }

        public List<UsrShopifyInventoryLevel> 
                    RetrieveInventoryLevels(long shopifyInventoryItemId)
        {
            return Entities
                    .UsrShopifyInventoryLevels
                    .Where(x => x.ShopifyInventoryItemId == shopifyInventoryItemId)
                    .ToList();
        }

        public void InsertInventoryLevel(UsrShopifyInventoryLevel level)
        {
            Entities.UsrShopifyInventoryLevels.Add(level);
            Entities.SaveChanges();
        }

        public List<UsrShopifyInventoryLevel> 
                        RetrieveInventoryLevelsMatchedButNotSynced()
        {
            return Entities
                    .UsrShopifyInventoryLevels
                    .Include(x => x.UsrShopifyVariant)
                    .Include(x => x.UsrShopifyVariant.UsrAcumaticaStockItems)
                    .Include(x => x.UsrShopifyVariant.UsrShopifyProduct)
                    .Include(x => x.UsrShopifyLocation)
                    .Where(x => x.UsrAcumaticaInventoryReceipt == null
                                && x.UsrShopifyVariant.UsrAcumaticaStockItems.Any()
                                && x.ShopifyAvailableQuantity > 0)
                    .ToList();
        }



        // Product to Stock Item matching 
        //
        public List<UsrShopifyVariant> RetrieveVariants(bool? isMatched = null)
        {
            var output = 
                Entities
                    .UsrShopifyVariants
                    .Include(x => x.UsrShopifyProduct)
                    .Include(x => x.UsrAcumaticaStockItems);

            if (isMatched.HasValue)
            {
                output = output.Where(x => x.UsrAcumaticaStockItems.Any() == isMatched);
            }

            return output.ToList();
        }

        public List<UsrShopifyVariant> RetrieveVariantsWithStockItems(string sku)
        {
            return Entities
                    .UsrShopifyVariants
                    .Include(x => x.UsrAcumaticaStockItems)
                    .Where(x => x.ShopifySku == sku)
                    .ToList();
        }

        public List<UsrShopifyVariant> 
                        RetrieveVariantsByParent(long parentMonsterId)
        {
            return Entities.UsrShopifyVariants
                    .Where(x => x.ParentMonsterId == parentMonsterId)
                    .ToList();
        }
        

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
