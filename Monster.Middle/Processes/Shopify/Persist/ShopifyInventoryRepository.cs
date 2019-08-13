using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Shopify.Persist
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
            return Entities.UsrShopifyLocations.ToList();
        }



        // Shopify persistence
        //
        public UsrShopifyProduct RetrieveProduct(long shopifyProductId)
        {
            return Entities
                .UsrShopifyProducts
                .FirstOrDefault(x => x.ShopifyProductId == shopifyProductId);
        }
        
        public UsrShopifyVariant RetrieveVariant(long shopifyVariantId)
        {
            return Entities
                .UsrShopifyVariants
                .Include(x => x.UsrShopifyInventoryLevels)
                .FirstOrDefault(x => x.ShopifyVariantId == shopifyVariantId);
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
                    RetrieveInventory(long shopifyInventoryItemId)
        {
            return Entities
                    .UsrShopifyInventoryLevels
                    .Where(x => x.ShopifyInventoryItemId == shopifyInventoryItemId)
                    .ToList();
        }

        public void InsertInventory(UsrShopifyInventoryLevel inventory)
        {
            Entities.UsrShopifyInventoryLevels.Add(inventory);
            Entities.SaveChanges();
        }



        // Product to Stock Item matching 
        //
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

