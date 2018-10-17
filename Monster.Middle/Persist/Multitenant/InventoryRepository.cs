using System;
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



        // Maaaybe we'll move this Location stuff elsewhere...

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
        
        public DateTime? RetrieveShopifyProductMaxUpdatedDate()
        {
            if (Entities.UsrShopifyPayouts.Any())
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


        public UsrAcumaticaStockItem
                    RetreiveAcumaticaStockItems(string itemId)
        {
            return Entities
                    .UsrAcumaticaStockItems
                    .FirstOrDefault(x => x.ItemId == itemId);
        }

        public void InsertAcumaticaStockItems(UsrAcumaticaStockItem item)
        {
            Entities.UsrAcumaticaStockItems.Add(item);
            Entities.SaveChanges();
        }

        public DateTime? RetrieveAcumaticaStockItemsMaxUpdatedDate()
        {
            if (Entities.UsrShopifyPayouts.Any())
            {
                return Entities.UsrAcumaticaStockItems
                    .Select(x => x.LastUpdated)
                    .Max();
            }
            else
            {
                return (DateTime?)null;
            }
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }

    }
}
