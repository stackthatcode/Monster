using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Push.Foundation.Utilities.General;

namespace Monster.Middle.Processes.Sync.Inventory
{
    public class SyncInventoryRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public SyncInventoryRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        public DbContextTransaction BeginTransaction()
        {
            return Entities.Database.BeginTransaction();
        }


        // Warehouses & Locations
        //
        public void InsertInventoryReceiptSync(
                UsrShopifyInventoryLevel level, UsrAcumaticaInventoryReceipt receipt)
        {
            var sync = new UsrInventoryReceiptSync();
            sync.UsrShopifyInventoryLevel = level;
            sync.UsrAcumaticaInventoryReceipt = receipt;
            sync.DateCreated = DateTime.Now;
            sync.LastUpdated = DateTime.Now;
            Entities.UsrInventoryReceiptSyncs.Add(sync);
            Entities.SaveChanges();
        }

        public void InsertWarehouseSync(
                UsrShopifyLocation location, UsrAcumaticaWarehouse warehouse, bool isNameMismatched = false)
        {
            var sync = new UsrShopAcuWarehouseSync();
            sync.UsrShopifyLocation = location;
            sync.UsrAcumaticaWarehouse = warehouse;
            sync.IsNameMismatched = isNameMismatched;
            sync.DateCreated = DateTime.UtcNow;
            sync.LastUpdated = DateTime.UtcNow;
            Entities.UsrShopAcuWarehouseSyncs.Add(sync);
            Entities.SaveChanges();
        }

        public void DeleteWarehouseSync(UsrShopAcuWarehouseSync sync)
        {
            Entities.UsrShopAcuWarehouseSyncs.Remove(sync);
            Entities.SaveChanges();
        }

        public UsrShopifyLocation RetrieveLocation(long shopifyLocationId)
        {
            return Entities
                .UsrShopifyLocations
                .Include(x => x.UsrShopAcuWarehouseSyncs)
                .Include(x => x.UsrShopAcuWarehouseSyncs.Select(y => y.UsrAcumaticaWarehouse))
                .FirstOrDefault(x => x.ShopifyLocationId == shopifyLocationId);
        }

        public List<UsrShopifyLocation> RetrieveLocations()
        {
            return Entities
                .UsrShopifyLocations
                .Include(x => x.UsrShopAcuWarehouseSyncs)
                .Include(x => x.UsrShopAcuWarehouseSyncs.Select(y => y.UsrAcumaticaWarehouse))
                .ToList();
        }

        public List<UsrAcumaticaWarehouse> RetrieveWarehouses()
        {
            return Entities
                .UsrAcumaticaWarehouses
                .Include(x => x.UsrShopAcuWarehouseSyncs)
                .Include(x => x.UsrShopAcuWarehouseSyncs.Select(y => y.UsrShopifyLocation))
                .ToList();
        }

        public List<UsrShopifyLocation> RetrieveLocations(bool? synced = null)
        {
            var output = Entities
                .UsrShopifyLocations
                .Include(x => x.UsrShopAcuWarehouseSyncs)
                .Include(x => x.UsrShopAcuWarehouseSyncs.Select(y => y.UsrAcumaticaWarehouse));

            if (synced.HasValue && synced.Value == true)
            {
                output = output.Where(x => x.UsrShopAcuWarehouseSyncs.Any());
            }
            if (synced.HasValue && synced.Value == false)
            {
                output = output.Where(x => x.UsrShopAcuWarehouseSyncs.Any());
            }

            return output.ToList();
        }


        public List<UsrAcumaticaWarehouse> RetrieveSyncWarehouses()
        {
            return Entities
                .UsrAcumaticaWarehouses
                .Include(x => x.UsrShopAcuWarehouseSyncs)
                .Include(x => x.UsrShopAcuWarehouseSyncs.Select(y => y.UsrShopifyLocation))
                .Where(x => x.UsrShopAcuWarehouseSyncs.Any())
                .ToList();
        }

        public UsrAcumaticaWarehouse RetrieveWarehouse(string warehouseId)
        {
            return Entities
                .UsrAcumaticaWarehouses
                .Include(x => x.UsrShopAcuWarehouseSyncs)
                .Include(x => x.UsrShopAcuWarehouseSyncs.Select(y => y.UsrShopifyLocation))
                .FirstOrDefault(x => x.AcumaticaWarehouseId == warehouseId);
        }


        
        // Products/Variants and Stock Items
        //
        public UsrShopifyVariant RetrieveVariant(long shopifyVariantId, string sku)
        {
            return Entities
                .UsrShopifyVariants
                .Include(x => x.UsrShopAcuItemSyncs)
                .Include(x => x.UsrShopAcuItemSyncs.Select(y => y.UsrAcumaticaStockItem))
                .FirstOrDefault(x => x.ShopifyVariantId == shopifyVariantId
                                     && x.ShopifySku == sku);
        }

        public UsrAcumaticaStockItem RetrieveStockItem(string itemId)
        {
            return Entities
                .UsrAcumaticaStockItems
                .Include(x => x.UsrShopAcuItemSyncs)
                .Include(x => x.UsrAcumaticaWarehouseDetails)
                .Include(x => x.UsrShopAcuItemSyncs.Select(y => y.UsrShopifyVariant))
                .FirstOrDefault(x => x.ItemId == itemId);
        }

        public List<UsrShopifyVariant> RetrieveUnmatchedVariants(long shopifyProductId)
        {
            var output 
                = Entities
                    .UsrShopifyVariants
                    .Include(x => x.UsrShopifyProduct)
                    .Include(x => x.UsrShopAcuItemSyncs)
                    .Include(x => x.UsrShopAcuItemSyncs.Select(y => y.UsrAcumaticaStockItem))
                    .Where(x => x.UsrShopifyProduct.ShopifyProductId == shopifyProductId)
                    .Where(x => !x.UsrShopAcuItemSyncs.Any());

            return output.ToList();
        }

        public List<UsrShopifyVariant> RetrieveVariantsWithStockItems(string sku)
        {
            return Entities
                .UsrShopifyVariants
                    .Include(x => x.UsrShopifyProduct)
                    .Include(x => x.UsrShopAcuItemSyncs)
                    .Include(x => x.UsrShopAcuItemSyncs.Select(y => y.UsrAcumaticaStockItem))
                    .Where(x => x.ShopifySku == sku)
                    .ToList();
        }

        public void InsertItemSync(
                UsrShopifyVariant variant, UsrAcumaticaStockItem stockItem, bool isSyncEnabled)
        {
            var sync = new UsrShopAcuItemSync();
            sync.UsrShopifyVariant = variant;
            sync.UsrAcumaticaStockItem = stockItem;
            sync.IsSyncEnabled = isSyncEnabled;
            sync.DateCreated = DateTime.UtcNow;
            sync.LastUpdated = DateTime.UtcNow;

            Entities.UsrShopAcuItemSyncs.Add(sync);
            Entities.SaveChanges();
        }


        // Inventory Price
        //
        public List<UsrAcumaticaStockItem> RetrieveStockItemsNotSynced()
        {
            return Entities
                    .UsrAcumaticaStockItems
                    .Include(x => x.UsrShopAcuItemSyncs)
                    .Where(x => x.IsPriceSynced == false)
                    .ToList();
        }


        // Inventory and Warehouse Details
        //
        public List<UsrShopifyInventoryLevel> RetrieveInventoryLevelsWithoutReceipts()
        {
            return Entities
                    .UsrShopifyInventoryLevels
                    .Include(x => x.UsrShopifyVariant)
                    .Include(x => x.UsrShopifyVariant.UsrShopAcuItemSyncs)
                    .Include(x => x.UsrShopifyLocation)
                    .Where(x => !x.UsrInventoryReceiptSyncs.Any() && x.ShopifyAvailableQuantity > 0)
                    .ToList();
        }

        public List<UsrShopifyInventoryLevel> RetrieveInventoryLevels(long shopifyProductId)
        {
            return Entities
                .UsrShopifyInventoryLevels
                .Include(x => x.UsrShopifyVariant)
                .Include(x => x.UsrShopifyVariant.UsrShopAcuItemSyncs)
                .Include(x => x.UsrShopifyLocation)
                .Where(x => x.UsrShopifyVariant.UsrShopifyProduct.ShopifyProductId == shopifyProductId)
                .ToList();
        }



        public List<UsrAcumaticaWarehouseDetail> RetrieveWarehouseDetailsNotSynced()
        {
            return Entities
                .UsrAcumaticaWarehouseDetails
                .Include(x => x.UsrAcumaticaStockItem)
                .Include(x => x.UsrAcumaticaStockItem.UsrShopAcuItemSyncs)
                .Where(x => x.UsrAcumaticaStockItem.UsrShopAcuItemSyncs.Any()
                            && x.IsInventorySynced == false)
                .ToList();
        }


        // Product/Variant-Stock Item Sync Control
        //
        public List<UsrShopAcuItemSync> 
                    SearchVariantAndStockItems(string filterText, int syncEnabledFilter)
        {
            var termList = filterText.Split(' ').Where(x => x.Trim() != "").ToList();

            var dataSet
                = Entities
                    .UsrShopAcuItemSyncs
                    .Include(x => x.UsrShopifyVariant)
                    .Include(x => x.UsrShopifyVariant.UsrShopifyProduct)
                    .Include(x => x.UsrAcumaticaStockItem);

            foreach (var term in termList)
            {
                dataSet = dataSet.Where(
                    x => x.UsrShopifyVariant.ShopifyTitle.Contains(term) ||
                         x.UsrShopifyVariant.ShopifySku.Contains(term) ||
                         x.UsrShopifyVariant.UsrShopifyProduct.ShopifyProductType.Contains(term) ||
                         x.UsrShopifyVariant.UsrShopifyProduct.ShopifyVendor.Contains(term) ||
                         x.UsrShopifyVariant.UsrShopifyProduct.ShopifyTitle.Contains(term) ||
                         x.UsrAcumaticaStockItem.ItemId.Contains(term) ||
                         x.UsrAcumaticaStockItem.AcumaticaDescription.Contains(term));
            }
            if (syncEnabledFilter == SyncEnabledFilter.EnabledOnly)
            {
                dataSet = dataSet.Where(x => x.IsSyncEnabled == true);
            }
            if (syncEnabledFilter == SyncEnabledFilter.DisabledOnly)
            {
                dataSet = dataSet.Where(x => x.IsSyncEnabled == false);
            }
            
            return dataSet.ToList();
        }

        public int RetrieveVariantAndStockItemMatchCount()
        {
            var sql = "SELECT COUNT(*) FROM vw_SyncVariantAndStockItem";

            return Entities
                .Database
                .SqlQuery<int>(sql)
                .First();
        }


        public UsrShopifyProduct RetrieveProduct(long shopifyProductId)
        {
            return Entities
                    .UsrShopifyProducts
                    .Include(x => x.UsrShopifyVariants)
                    .Include(x => x.UsrShopifyVariants.Select(y => y.UsrShopAcuItemSyncs))
                    .Include(x => x.UsrShopifyVariants.Select(y => y.UsrShopifyInventoryLevels))
                    .FirstOrDefault(x => x.ShopifyProductId == shopifyProductId);
        }

        public List<UsrShopifyProduct> ProductSearch(string terms)
        {
            var termList 
                = terms.Split(' ')
                    .Where(x => x.Trim() != "")
                    .ToList();

            var dataSet
                = Entities
                    .UsrShopifyProducts
                    .Include(x => x.UsrShopifyVariants)
                    .Include(x => x.UsrShopifyVariants.Select(y => y.UsrShopAcuItemSyncs))
                    .Where(x => x.UsrShopifyVariants.Any(y => !y.UsrShopAcuItemSyncs.Any()));
            
            foreach (var term in termList)
            {
                dataSet = dataSet.Where(
                    x => x.ShopifyTitle.Contains(term) ||
                         x.ShopifyProductType.Contains(term) ||
                         x.ShopifyVendor.Contains(term) ||
                         x.UsrShopifyVariants.Any(y => y.ShopifySku.Contains(term)) ||
                         x.UsrShopifyVariants.Any(y => y.ShopifyTitle.Contains(term)));
            }

            return dataSet.ToList();
        }

        public void UpdateVariantSync(long monsterVariantId, bool syncEnabled)
        {
            var sync = 
                Entities.UsrShopAcuItemSyncs
                        .First(x => x.ShopifyVariantMonsterId == monsterVariantId);
            sync.IsSyncEnabled = syncEnabled;
            Entities.SaveChanges();
        }

        public void UpdateVariantSync(List<long> monsterVariantIds, bool syncEnabled)
        {
            var variants =
                Entities.UsrShopAcuItemSyncs
                    .Where(x => monsterVariantIds.Contains(x.ShopifyVariantMonsterId))
                    .ToList();

            variants.ForEach(x => x.IsSyncEnabled = syncEnabled);
            Entities.SaveChanges();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
