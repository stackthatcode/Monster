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

        public List<UsrShopifyInventoryLevel> 
                    RetrieveShopifyInventoryLevels(long shopifyInventoryItemId)
        {
            return Entities
                    .UsrShopifyInventoryLevels
                    .Where(x => x.ShopifyInventoryItemId == shopifyInventoryItemId)
                    .ToList();
        }

        public void InsertShopifyInventoryLevel(UsrShopifyInventoryLevel level)
        {
            Entities.UsrShopifyInventoryLevels.Add(level);
            Entities.SaveChanges();
        }

        public List<UsrShopifyInventoryLevel> 
                        RetrieveShopifyInventoryLevelsMatchedButNotSynced()
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

        public List<UsrAcumaticaWarehouseDetail> 
                    RetrieveAcumaticaWarehouseDetails(long stockItemMonstedId)
        {
            return Entities
                .UsrAcumaticaWarehouseDetails
                .Where(x => x.ParentMonsterId == stockItemMonstedId)
                .ToList();
        }

        public List<UsrAcumaticaWarehouseDetail>
                        RetrieveAcumaticaWarehouseDetailsNotSynced()
        {
            return Entities
                    .UsrAcumaticaWarehouseDetails
                    .Include(x => x.UsrAcumaticaStockItem)
                    .Include(x => x.UsrAcumaticaStockItem.UsrShopifyVariant)
                    .Include(x => x.UsrAcumaticaStockItem.UsrShopifyVariant.UsrAcumaticaStockItems)
                    .Where(x => x.ShopifyIsSynced == false &&
                                x.UsrAcumaticaStockItem != null &&
                                x.UsrAcumaticaStockItem.UsrShopifyVariant != null &&
                                x.UsrAcumaticaStockItem.UsrShopifyVariant.ShopifyIsTracked)
                    .ToList();
        }

        public void InsertAcumaticaWarehouseDetails(UsrAcumaticaWarehouseDetail details)
        {
            Entities.UsrAcumaticaWarehouseDetails.Add(details);
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

        public void InsertAcumaticaStockItems(UsrAcumaticaStockItem item)
        {
            Entities.UsrAcumaticaStockItems.Add(item);
            Entities.SaveChanges();
        }


        public void InsertAcumaticaInventoryReceipt(UsrAcumaticaInventoryReceipt receipt)
        {
            Entities.UsrAcumaticaInventoryReceipts.Add(receipt);
            Entities.SaveChanges();
        }

        public void UpdateAcumaticaInventoryReceipt(
                        List<UsrShopifyInventoryLevel> levels,
                        UsrAcumaticaInventoryReceipt receipt)
        {
            levels.ForEach(x => x.InventoryReceiptMonsterId = receipt.MonsterId);
            Entities.SaveChanges();
        }

        public List<UsrAcumaticaInventoryReceipt> 
                        RetrieveNonReleasedAcumaticaInventoryReceipts()
        {
            return Entities
                    .UsrAcumaticaInventoryReceipts
                    .Where(x => x.IsReleased == false)
                    .ToList();
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
