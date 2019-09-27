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
        private readonly InstancePersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public ShopifyInventoryRepository(InstancePersistContext dataContext)
        {
            _dataContext = dataContext;
        }



        public void InsertLocation(ShopifyLocation location)
        {
            Entities.ShopifyLocations.Add(location);
            Entities.SaveChanges();
        }

        public IList<ShopifyLocation> RetreiveLocations()
        {
            return Entities.ShopifyLocations.ToList();
        }



        // Shopify persistence
        //
        public ShopifyProduct RetrieveProduct(long shopifyProductId)
        {
            return Entities
                .ShopifyProducts
                .FirstOrDefault(x => x.ShopifyProductId == shopifyProductId);
        }
        
        public ShopifyVariant RetrieveVariant(long shopifyVariantId)
        {
            return Entities
                .ShopifyVariants
                .Include(x => x.ShopifyInventoryLevels)
                .FirstOrDefault(x => x.ShopifyVariantId == shopifyVariantId);
        }
        

        public void InsertProduct(ShopifyProduct product)
        {
            Entities.ShopifyProducts.Add(product);
            Entities.SaveChanges();
        }

        public void InsertVariant(ShopifyVariant variant)
        {
            Entities.ShopifyVariants.Add(variant);
            Entities.SaveChanges();
        }

        public List<ShopifyInventoryLevel> 
                    RetrieveInventory(long shopifyInventoryItemId)
        {
            return Entities
                    .ShopifyInventoryLevels
                    .Where(x => x.ShopifyInventoryItemId == shopifyInventoryItemId)
                    .ToList();
        }

        public void InsertInventory(ShopifyInventoryLevel inventory)
        {
            Entities.ShopifyInventoryLevels.Add(inventory);
            Entities.SaveChanges();
        }



        // Product to Stock Item matching 
        //
        public List<ShopifyVariant> 
                        RetrieveVariantsByParent(long parentMonsterId)
        {
            return Entities.ShopifyVariants
                    .Where(x => x.ParentMonsterId == parentMonsterId)
                    .ToList();
        }
        
        public void SaveChanges()
        {
            Entities.SaveChanges();
        }

    }
}

