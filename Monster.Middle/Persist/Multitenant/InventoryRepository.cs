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



        // Shopify persistence
        //
        public UsrShopifyProduct 
                RetrieveShopifyProduct(long shopifyProductId)
        {
            return Entities
                .UsrShopifyProducts
                .FirstOrDefault(
                        x => x.ShopifyProductId == shopifyProductId);
        }
        
        public UsrShopifyVariant
                RetrieveShopifyVariants(long shopifyVariantId, string sku)
        {
            return Entities
                .UsrShopifyVariants
                .FirstOrDefault(
                    x => x.ShopifyVariantId == shopifyVariantId &&
                         x.ShopifySku == sku);
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

        public void InsertShopifyVariant(UsrShopifyVariant variant)
        {
            Entities.UsrShopifyVariants.Add(variant);
            Entities.SaveChanges();
        }




        // Acumatica persistence
        //
        public UsrAcumaticaStockItem
                    RetreiveAcumaticaStockItems(string itemId)
        {
            return Entities
                    .UsrAcumaticaStockItems
                    .FirstOrDefault(x => x.ItemId == itemId);
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

        public void InsertAcumaticaStockItems(UsrAcumaticaStockItem item)
        {
            Entities.UsrAcumaticaStockItems.Add(item);
            Entities.SaveChanges();
        }



        // Product to Stock Item matching 
        //
        public List<UsrShopifyVariant> RetrieveUnmatchedVariants()
        {
            return Entities
                    .UsrShopifyVariants
                    .Where(x => !x.UsrProductMatches.Any())
                    .ToList();
        }

        public List<UsrShopifyVariant> RetrieveShopifyVariants(string sku)
        {
            return Entities
                    .UsrShopifyVariants
                    .Where(x => x.ShopifySku == sku)
                    .ToList();
        }


        // Matching data
        //
        public void InsertMatch(
            long shopifyVariantMonsterId,
            long acumaticaStockItemMonsterId)
        {
            var match = new UsrProductMatch()
            {
                ShopifyVariantMonsterId = shopifyVariantMonsterId,
                AcumaticaStockItemMonsterId = acumaticaStockItemMonsterId,
            };

            Entities.UsrProductMatches.Add(match);
            Entities.SaveChanges();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }

    }
}
