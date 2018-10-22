using System;
using System.Data.Entity;
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
                RetrieveShopifyVariant(long shopifyVariantId, string sku)
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
                    RetreiveAcumaticaStockItem(string itemId, bool? isMatched = null)
        {
            if (isMatched.HasValue)
            {
                return Entities
                    .UsrAcumaticaStockItems
                    .FirstOrDefault(x => x.ShopifyVariantMonsterId.HasValue == isMatched
                                    && x.ItemId == itemId);
            }
            else
            {
                return Entities
                    .UsrAcumaticaStockItems
                    .FirstOrDefault(x => x.ItemId == itemId);
            }
        }

        public UsrAcumaticaStockItem
                    RetreiveAcumaticaStockItem(long monsterId)
        {
            return Entities
                    .UsrAcumaticaStockItems
                    .FirstOrDefault(x => x.MonsterId == monsterId);
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
        public List<UsrShopifyVariant> RetrieveShopifyVariants(bool? isMatched = null)
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

        public List<UsrShopifyVariant> RetrieveShopifyVariants(string sku)
        {
            return Entities
                    .UsrShopifyVariants
                    .Include(x => x.UsrAcumaticaStockItems)
                    .Where(x => x.ShopifySku == sku)
                    .ToList();
        }

        public List<UsrShopifyVariant> 
                        RetrieveShopifyVariantsByParent(long parentMonsterId)
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
