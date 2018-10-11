using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant
{
    public class InventoryRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public InventoryRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }


        public void InsertShopifyLocation(UsrShopifyLocation location)
        {
            Entities.UsrShopifyLocations.Add(location);
            Entities.SaveChanges();
        }

        public IList<UsrShopifyLocation> RetreiveShopifyLocations()
        {
            return Entities.UsrShopifyLocations.ToList();
        }


        public void InsertAcumaticaWarehouse(UsrAcumaticaWarehouse warehouse)
        {
            Entities.UsrAcumaticaWarehouses.Add(warehouse);
            Entities.SaveChanges();
        }

        public IList<UsrAcumaticaWarehouse> RetreiveAcumaticaWarehouses()
        {
            return Entities.UsrAcumaticaWarehouses.ToList();
        }


        public UsrShopifyProduct 
                RetrieveShopifyProduct(long shopifyProductId)
        {
            return Entities
                .UsrShopifyProducts
                .FirstOrDefault(
                        x => x.ShopifyProductId == shopifyProductId);
        }

        public void InsertShopifyProduct(UsrShopifyProduct product)
        {
            Entities.UsrShopifyProducts.Add(product);
            Entities.SaveChanges();
        }

        public UsrShopifyVariant 
                    RetrieveShopifyVariants(
                        long shopifyVariantId, string sku)
        {
            return Entities
                .UsrShopifyVariants
                .FirstOrDefault(
                    x => x.ShopifyVariantId == shopifyVariantId &&
                         x.ShopifySku == sku);
        }

        public void InsertShopifyVariant(UsrShopifyVariant variant)
        {
            Entities.UsrShopifyVariants.Add(variant);
            Entities.SaveChanges();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }

    }
}
