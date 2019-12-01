using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Status;
using Z.EntityFramework.Plus;


namespace Monster.Middle.Processes.Sync.Persist
{
    public class SyncInventoryRepository
    {
        private readonly ProcessPersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public SyncInventoryRepository(ProcessPersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        public DbContextTransaction BeginTransaction()
        {
            return Entities.Database.BeginTransaction();
        }


        // Warehouses & Locations
        //
        public void InsertInventoryReceiptSync(ShopifyInventoryLevel level, AcumaticaInventoryReceipt receipt)
        {
            var sync = new InventoryReceiptSync();
            sync.ShopifyInventoryLevel = level;
            sync.AcumaticaInventoryReceipt = receipt;
            sync.DateCreated = DateTime.Now;
            sync.LastUpdated = DateTime.Now;
            Entities.InventoryReceiptSyncs.Add(sync);
            Entities.SaveChanges();
        }


        public void ImprintWarehouseSync(string warehouseId, long? locationId)
        {
            var warehouse = RetrieveWarehouse(warehouseId);

            // Deletion when a NULL location is selected
            if (!locationId.HasValue)
            {
                if (warehouse.HasMatch())
                {
                    var sync = warehouse.ShopAcuWarehouseSyncs.First();
                    DeleteWarehouseSync(sync);
                }

                return;
            }
            
            // Location is non-NULL
            if (warehouse.HasMatch())
            {
                if (warehouse.MatchedLocation().ShopifyLocationId == locationId.Value)
                {
                    // The current sync is the same one - FIN
                    return;
                }
                else
                {
                    var sync = warehouse.ShopAcuWarehouseSyncs.First();
                    DeleteWarehouseSync(sync);
                }
            }

            // Create a new Location-Warehouse Sync
            var location = RetrieveLocation(locationId.Value);
            InsertWarehouseSync(location, warehouse);
        }


        public void InsertWarehouseSync(ShopifyLocation location, AcumaticaWarehouse warehouse)
        {
            var sync = new ShopAcuWarehouseSync();
            sync.ShopifyLocation = location;
            sync.AcumaticaWarehouse = warehouse;
            sync.DateCreated = DateTime.UtcNow;
            sync.LastUpdated = DateTime.UtcNow;
            Entities.ShopAcuWarehouseSyncs.Add(sync);
            Entities.SaveChanges();
        }

        public void DeleteWarehouseSync(ShopAcuWarehouseSync sync)
        {
            Entities.ShopAcuWarehouseSyncs.Remove(sync);
            Entities.SaveChanges();
        }

        public ShopifyLocation RetrieveLocation(long shopifyLocationId)
        {
            return Entities
                .ShopifyLocations
                .Include(x => x.ShopAcuWarehouseSyncs)
                .Include(x => x.ShopAcuWarehouseSyncs.Select(y => y.AcumaticaWarehouse))
                .FirstOrDefault(x => x.ShopifyLocationId == shopifyLocationId);
        }

        public List<ShopifyLocation> RetrieveLocations()
        {
            return Entities
                .ShopifyLocations
                .Where(x => x.ShopifyActive == true)
                .Include(x => x.ShopAcuWarehouseSyncs)
                .Include(x => x.ShopAcuWarehouseSyncs.Select(y => y.AcumaticaWarehouse))
                .ToList();
        }

        public List<ShopifyLocation> RetrieveDeactivatedMatchedLocations()
        {
            return Entities
                .ShopifyLocations
                .Include(x => x.ShopAcuWarehouseSyncs)
                .Include(x => x.ShopAcuWarehouseSyncs.Select(y => y.AcumaticaWarehouse))
                .Where(x => x.ShopifyActive == false)
                .Where(x => x.ShopAcuWarehouseSyncs.Any())
                .ToList();
        }

        public List<AcumaticaWarehouse> RetrieveWarehouses()
        {
            return Entities
                .AcumaticaWarehouses
                .Include(x => x.ShopAcuWarehouseSyncs)
                .Include(x => x.ShopAcuWarehouseSyncs.Select(y => y.ShopifyLocation))
                .ToList();
        }

        public List<ShopifyLocation> RetrieveMatchedLocations()
        {
            var output = Entities
                .ShopifyLocations
                .Include(x => x.ShopAcuWarehouseSyncs)
                .Include(x => x.ShopAcuWarehouseSyncs.Select(y => y.AcumaticaWarehouse))
                .Where(x => x.ShopAcuWarehouseSyncs.Any());

            return output.ToList();
        }
        
        public List<AcumaticaWarehouse> RetrieveMatchedWarehouses()
        {
            return Entities
                .AcumaticaWarehouses
                .Include(x => x.ShopAcuWarehouseSyncs)
                .Include(x => x.ShopAcuWarehouseSyncs.Select(y => y.ShopifyLocation))
                .Where(x => x.ShopAcuWarehouseSyncs.Any())
                .ToList();
        }

        public AcumaticaWarehouse RetrieveWarehouse(string warehouseId)
        {
            return Entities
                .AcumaticaWarehouses
                .Include(x => x.ShopAcuWarehouseSyncs)
                .Include(x => x.ShopAcuWarehouseSyncs.Select(y => y.ShopifyLocation))
                .FirstOrDefault(x => x.AcumaticaWarehouseId == warehouseId);
        }


        
        // Products/Variants and Stock Items
        //
        public ShopifyVariant RetrieveVariant(long shopifyVariantId, string sku)
        {
            return Entities
                .ShopifyVariants
                .Include(x => x.AcumaticaStockItems)
                .FirstOrDefault(x => x.ShopifyVariantId == shopifyVariantId && x.ShopifySku == sku);
        }

        public ShopifyVariant RetrieveLiveVariant(string sku)
        {
            return Entities
                .ShopifyVariants
                .Include(x => x.AcumaticaStockItems)
                .FirstOrDefault(x => x.IsMissing == false 
                                     && x.AcumaticaStockItems.Any() 
                                     && x.ShopifySku == sku);
        }

        public ShopifyVariant RetrieveLiveVariant(long shopifyProductId, string sku)
        {
            return Entities
                .ShopifyVariants
                .Where(x => x.ShopifyProduct.ShopifyProductId == shopifyProductId)
                .Include(x => x.AcumaticaStockItems)
                .FirstOrDefault(x => x.IsMissing == false
                                     && x.AcumaticaStockItems.Any()
                                     && x.ShopifySku == sku);
        }

        public AcumaticaStockItem RetrieveStockItem(string itemId)
        {
            return Entities
                .AcumaticaStockItems
                .Include(x => x.AcumaticaInventories)
                .Include(x => x.ShopifyVariant)
                .FirstOrDefault(x => x.ItemId == itemId);
        }

        public List<ShopifyVariant> RetrieveUnmatchedVariants(long shopifyProductId)
        {
            var output 
                = Entities
                    .ShopifyVariants
                    .Include(x => x.ShopifyProduct)
                    .Include(x => x.ShopifyInventoryLevels)
                    .Where(x => x.IsMissing == false)
                    .Where(x => x.ShopifyProduct.ShopifyProductId == shopifyProductId)
                    .Where(x => !x.AcumaticaStockItems.Any());

            return output.ToList();
        }

        public List<ShopifyVariant> RetrieveNonMissingVariants(string sku)
        {
            return Entities
                .ShopifyVariants
                    .Include(x => x.ShopifyProduct)
                    .Include(x => x.AcumaticaStockItems)
                    .Where(x => x.ShopifySku == sku && x.IsMissing == false)
                    .ToList();
        }

        public void InsertItemSync(ShopifyVariant variant, AcumaticaStockItem stockItem, bool isSyncEnabled)
        {
            stockItem.ShopifyVariantMonsterId = variant.MonsterId;
            stockItem.IsSyncEnabled = isSyncEnabled;
            stockItem.LastUpdated = DateTime.UtcNow;
            Entities.SaveChanges();
        }

        public void DeleteItemSyncs(AcumaticaStockItem stockItem)
        {
            stockItem.IsSyncEnabled = false;
            stockItem.ShopifyVariantMonsterId = null;
            Entities.SaveChanges();
        }


        // Inventory Price
        //
        public List<AcumaticaStockItem> RetrieveStockItemsPriceNotSynced()
        {
            return Entities
                    .AcumaticaStockItems
                    .Include(x => x.ShopifyVariant)
                    .Where(x => x.IsPriceSynced == false && x.IsSyncEnabled == true)
                    .ToList();
        }
        
        // Inventory and Warehouse Details
        //
        public List<ShopifyInventoryLevel> RetrieveInventoryLevelsWithoutReceipts()
        {
            return Entities
                    .ShopifyInventoryLevels
                    .Include(x => x.ShopifyVariant)
                    .Include(x => x.ShopifyVariant.AcumaticaStockItems)
                    .Include(x => x.ShopifyLocation)
                    .Where(x => !x.InventoryReceiptSyncs.Any() && x.ShopifyAvailableQuantity > 0)
                    .ToList();
        }

        public List<ShopifyInventoryLevel> RetrieveInventoryLevels(long shopifyProductId)
        {
            return Entities
                .ShopifyInventoryLevels
                .Include(x => x.ShopifyVariant)
                .Include(x => x.ShopifyVariant.AcumaticaStockItems)
                .Include(x => x.ShopifyLocation)
                .Where(x => x.ShopifyVariant.ShopifyProduct.ShopifyProductId == shopifyProductId)
                .ToList();
        }
        
        public List<AcumaticaStockItem> RetrieveStockItemInventoryNotSynced()
        {
            return Entities
                .AcumaticaStockItems
                .Include(x => x.AcumaticaInventories)
                .Include(x => x.ShopifyVariant)
                .Where(x => x.ShopifyVariant.IsMissing == false
                            && x.IsSyncEnabled == true
                            && x.AcumaticaInventories.Any(y => y.IsInventorySynced == false))
                .ToList();
        }



        // Product/Variant-Stock Item Sync Control
        //
        public List<AcumaticaStockItem> SearchSyncedStockItemsResults(
                string filterText, int syncEnabledFilter, int startRecord, int pageSize)
        {
            var dataSet = SearchSyncedStockItems(filterText, syncEnabledFilter);

            dataSet = dataSet
                .OrderBy(x => x.ShopifyVariant.ShopifySku)
                .Skip(startRecord)
                .Take(pageSize);

            return dataSet.ToList();
        }

        public int SearchSyncedStockItemsCount(string filterText, int syncEnabledFilter)
        {
            return SearchSyncedStockItems(filterText, syncEnabledFilter).Count();
        }

        private IQueryable<AcumaticaStockItem> 
                    SearchSyncedStockItems(string filterText, int syncEnabledFilter)
        {
            var termList = filterText.Split(' ').Where(x => x.Trim() != "").ToList();

            var dataSet
                = Entities
                    .AcumaticaStockItems
                    .Where(x => x.ShopifyVariant != null)
                    .Include(x => x.ShopifyVariant)
                    .Include(x => x.ShopifyVariant.ShopifyProduct);

            foreach (var term in termList)
            {
                dataSet = dataSet.Where(
                    x => x.ShopifyVariant.ShopifyTitle.Contains(term) ||
                         x.ShopifyVariant.ShopifySku.Contains(term) ||
                         x.ShopifyVariant.ShopifyProduct.ShopifyProductType.Contains(term) ||
                         x.ShopifyVariant.ShopifyProduct.ShopifyVendor.Contains(term) ||
                         x.ShopifyVariant.ShopifyProduct.ShopifyTitle.Contains(term) ||
                         x.ItemId.Contains(term) ||
                         x.AcumaticaDescription.Contains(term));
            }
            if (syncEnabledFilter == SyncEnabledFilter.EnabledOnly)
            {
                dataSet = dataSet.Where(x => x.IsSyncEnabled == true);
            }
            if (syncEnabledFilter == SyncEnabledFilter.DisabledOnly)
            {
                dataSet = dataSet.Where(x => x.IsSyncEnabled == false);
            }

            return dataSet;
        }



        public ShopifyProduct RetrieveProduct(long shopifyProductId)
        {
            return Entities
                    .ShopifyProducts
                    .Include(x => x.ShopifyVariants)
                    .Include(x => x.ShopifyVariants.Select(y => y.AcumaticaStockItems))
                    .Include(x => x.ShopifyVariants.Select(y => y.ShopifyInventoryLevels))
                    .FirstOrDefault(x => x.ShopifyProductId == shopifyProductId);
        }


        private IQueryable<ShopifyProduct> 
                    ProductSearchQueryable(string terms, bool onlyHavingUnsyncedVariants)
        {
            var dataSet = Entities.ShopifyProducts
                .Include(x => x.ShopifyVariants)
                .Include(x => x.ShopifyVariants.Select(y => y.AcumaticaStockItems));

            if (onlyHavingUnsyncedVariants)
            {
                // *** SAVE THIS... FOR NOW
                //
                dataSet = dataSet
                    .Where(x => x.ShopifyVariants.Any(
                            y => y.IsMissing == false && !y.AcumaticaStockItems.Any()));
            }

            var termList = terms.Split(' ').Where(x => x.Trim() != "").ToList();

            foreach (var term in termList)
            {
                dataSet = dataSet.Where(
                    x => x.ShopifyTitle.Contains(term) ||
                         x.ShopifyProductType.Contains(term) ||
                         x.ShopifyVendor.Contains(term) ||
                         x.ShopifyVariants.Any(y => y.ShopifySku.Contains(term)) ||
                         x.ShopifyVariants.Any(y => y.ShopifyTitle.Contains(term)));
            }

            return dataSet;
        }

        public List<ShopifyProduct> ProductSearchRecords(
                string terms, bool onlyHavingUnsyncedVariants, int startingRecord, int pageSize)
        {
            return ProductSearchQueryable(terms, onlyHavingUnsyncedVariants)
                .OrderBy(x => x.ShopifyTitle)
                .Skip(startingRecord)
                .Take(pageSize)
                .ToList();
        }

        public int ProductSearchCount(string terms, bool onlyHavingUnsyncedVariants)
        {
            return ProductSearchQueryable(terms, onlyHavingUnsyncedVariants).Count();
        }


        private IQueryable<AcumaticaStockItem> StockItemSearchQueryable(string terms)
        {
            var termList
                = terms.Split(' ')
                    .Where(x => x.Trim() != "")
                    .ToList();

            var dataSet
                = Entities
                    .AcumaticaStockItems
                    .Include(x => x.AcumaticaInventories)
                    .Where(x => x.ShopifyVariant == null);

            var settings = Entities.MonsterSettings.First();
            dataSet = dataSet.Where(x => x.AcumaticaTaxCategory == settings.AcumaticaTaxableCategory ||
                                         x.AcumaticaTaxCategory == settings.AcumaticaTaxableCategory);

            foreach (var term in termList)
            {
                dataSet = dataSet.Where(
                    x => x.AcumaticaDescription.Contains(term) ||
                         x.ItemId.Contains(term));
            }

            return dataSet;
        }

        public List<AcumaticaStockItem> StockItemSearchRecords(string terms, int startingRecord, int pageSize)
        {
            return StockItemSearchQueryable(terms)
                .OrderBy(x => x.ItemId)
                .Skip(startingRecord)
                .Take(pageSize)
                .ToList();
        }

        public int StockItemSearchCount(string terms)
        {
            return StockItemSearchQueryable(terms).Count();
        }


        public void UpdateVariantSync(long monsterVariantId, bool syncEnabled)
        {
            var sync = Entities
                .AcumaticaStockItems
                .First(x => x.ShopifyVariantMonsterId == monsterVariantId);
            sync.IsSyncEnabled = syncEnabled;
            Entities.SaveChanges();
        }

        public void UpdateVariantSync(List<long> monsterVariantIds, bool syncEnabled)
        {
            var variants =
                Entities.AcumaticaStockItems
                    .Where(x => x.ShopifyVariantMonsterId != null)
                    .Where(x => monsterVariantIds.Contains(x.ShopifyVariantMonsterId.Value))
                    .ToList();

            variants.ForEach(x => x.IsSyncEnabled = syncEnabled);
            Entities.SaveChanges();
        }

        public bool SkuExistsInShopify(string sku)
        {
            return Entities.ShopifyVariants.Any(x => x.ShopifySku == sku);
        }


        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
