using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Status;


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
                .Include(x => x.ShopAcuItemSyncs)
                .Include(x => x.ShopAcuItemSyncs.Select(y => y.AcumaticaStockItem))
                .FirstOrDefault(x => x.ShopifyVariantId == shopifyVariantId && x.ShopifySku == sku);
        }

        public AcumaticaStockItem RetrieveStockItem(string itemId)
        {
            return Entities
                .AcumaticaStockItems
                .Include(x => x.ShopAcuItemSyncs)
                .Include(x => x.AcumaticaWarehouseDetails)
                .Include(x => x.ShopAcuItemSyncs.Select(y => y.ShopifyVariant))
                .FirstOrDefault(x => x.ItemId == itemId);
        }

        public List<ShopifyVariant> RetrieveUnmatchedVariants(long shopifyProductId)
        {
            var output 
                = Entities
                    .ShopifyVariants
                    .Include(x => x.ShopifyProduct)
                    .Include(x => x.ShopAcuItemSyncs)
                    .Include(x => x.ShopAcuItemSyncs.Select(y => y.AcumaticaStockItem))
                    .Where(x => x.ShopifyProduct.ShopifyProductId == shopifyProductId)
                    .Where(x => !x.ShopAcuItemSyncs.Any());

            return output.ToList();
        }

        public List<ShopifyVariant> RetrieveVariantsWithStockItems(string sku)
        {
            return Entities
                .ShopifyVariants
                    .Include(x => x.ShopifyProduct)
                    .Include(x => x.ShopAcuItemSyncs)
                    .Include(x => x.ShopAcuItemSyncs.Select(y => y.AcumaticaStockItem))
                    .Where(x => x.ShopifySku == sku)
                    .ToList();
        }

        public void InsertItemSync(
                ShopifyVariant variant, AcumaticaStockItem stockItem, bool isSyncEnabled)
        {
            var sync = new ShopAcuItemSync();
            sync.ShopifyVariant = variant;
            sync.AcumaticaStockItem = stockItem;
            sync.IsSyncEnabled = isSyncEnabled;
            sync.DateCreated = DateTime.UtcNow;
            sync.LastUpdated = DateTime.UtcNow;

            Entities.ShopAcuItemSyncs.Add(sync);
            Entities.SaveChanges();
        }


        // Inventory Price
        //
        public List<AcumaticaStockItem> RetrieveStockItemsPriceNotSynced()
        {
            return Entities
                    .AcumaticaStockItems
                    .Include(x => x.ShopAcuItemSyncs)
                    .Include(x => x.ShopAcuItemSyncs.Select(y => y.ShopifyVariant))
                    .Where(x => x.IsPriceSynced == false)
                    .Where(x => x.ShopAcuItemSyncs.Any(y => y.IsSyncEnabled))
                    .ToList();
        }
        
        // Inventory and Warehouse Details
        //
        public List<ShopifyInventoryLevel> RetrieveInventoryLevelsWithoutReceipts()
        {
            return Entities
                    .ShopifyInventoryLevels
                    .Include(x => x.ShopifyVariant)
                    .Include(x => x.ShopifyVariant.ShopAcuItemSyncs)
                    .Include(x => x.ShopifyLocation)
                    .Where(x => !x.InventoryReceiptSyncs.Any() && x.ShopifyAvailableQuantity > 0)
                    .ToList();
        }

        public List<ShopifyInventoryLevel> RetrieveInventoryLevels(long shopifyProductId)
        {
            return Entities
                .ShopifyInventoryLevels
                .Include(x => x.ShopifyVariant)
                .Include(x => x.ShopifyVariant.ShopAcuItemSyncs)
                .Include(x => x.ShopifyLocation)
                .Where(x => x.ShopifyVariant.ShopifyProduct.ShopifyProductId == shopifyProductId)
                .ToList();
        }
        
        public List<AcumaticaStockItem> RetrieveStockItemInventoryNotSynced()
        {
            return Entities
                .AcumaticaStockItems
                .Include(x => x.AcumaticaWarehouseDetails)
                .Include(x => x.ShopAcuItemSyncs)
                .Include(x => x.ShopAcuItemSyncs.Select(y => y.ShopifyVariant))
                .Where(x => x.ShopAcuItemSyncs.Any(y => y.IsSyncEnabled)
                            && x.AcumaticaWarehouseDetails
                                    .Any(y => y.IsInventorySynced == false))
                .ToList();
        }



        // Product/Variant-Stock Item Sync Control
        //
        public List<ShopAcuItemSync> 
                    SearchVariantAndStockItems(string filterText, int syncEnabledFilter)
        {
            var termList = filterText.Split(' ').Where(x => x.Trim() != "").ToList();

            var dataSet
                = Entities
                    .ShopAcuItemSyncs
                    .Include(x => x.ShopifyVariant)
                    .Include(x => x.ShopifyVariant.ShopifyProduct)
                    .Include(x => x.AcumaticaStockItem);

            foreach (var term in termList)
            {
                dataSet = dataSet.Where(
                    x => x.ShopifyVariant.ShopifyTitle.Contains(term) ||
                         x.ShopifyVariant.ShopifySku.Contains(term) ||
                         x.ShopifyVariant.ShopifyProduct.ShopifyProductType.Contains(term) ||
                         x.ShopifyVariant.ShopifyProduct.ShopifyVendor.Contains(term) ||
                         x.ShopifyVariant.ShopifyProduct.ShopifyTitle.Contains(term) ||
                         x.AcumaticaStockItem.ItemId.Contains(term) ||
                         x.AcumaticaStockItem.AcumaticaDescription.Contains(term));
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


        public ShopifyProduct RetrieveProduct(long shopifyProductId)
        {
            return Entities
                    .ShopifyProducts
                    .Include(x => x.ShopifyVariants)
                    .Include(x => x.ShopifyVariants.Select(y => y.ShopAcuItemSyncs))
                    .Include(x => x.ShopifyVariants.Select(y => y.ShopifyInventoryLevels))
                    .FirstOrDefault(x => x.ShopifyProductId == shopifyProductId);
        }


        private IQueryable<ShopifyProduct> ProductSearchQueryable(string terms, bool includeSynced = false)
        {
            var termList
                = terms.Split(' ')
                    .Where(x => x.Trim() != "")
                    .ToList();

            var dataSet
                = Entities
                    .ShopifyProducts
                    .Include(x => x.ShopifyVariants)
                    .Include(x => x.ShopifyVariants.Select(y => y.ShopAcuItemSyncs));

            if (!includeSynced)
            {
                dataSet = dataSet.Where(x => x.ShopifyVariants.Any(y => !y.ShopAcuItemSyncs.Any()));
            }


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
                string terms, bool includeSynced, int startingRecord, int pageSize)
        {
            return ProductSearchQueryable(terms)
                .OrderBy(x => x.ShopifyTitle)
                .Skip(startingRecord)
                .Take(pageSize)
                .ToList();
        }

        public int ProductSearchCount(string terms, bool includeSynced = false)
        {
            return ProductSearchQueryable(terms).Count();
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
                    .Include(x => x.AcumaticaWarehouseDetails)
                    .Where(x => !x.ShopAcuItemSyncs.Any());

            foreach (var term in termList)
            {
                dataSet = dataSet.Where(
                    x => x.AcumaticaDescription.Contains(term) ||
                         x.ItemId.Contains(term));
            }

            return dataSet;
        }

        public List<AcumaticaStockItem> 
                    StockItemSearchRecords(string terms, int startingRecord, int pageSize)
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
            var sync = 
                Entities.ShopAcuItemSyncs
                        .First(x => x.ShopifyVariantMonsterId == monsterVariantId);
            sync.IsSyncEnabled = syncEnabled;
            Entities.SaveChanges();
        }

        public void UpdateVariantSync(List<long> monsterVariantIds, bool syncEnabled)
        {
            var variants =
                Entities.ShopAcuItemSyncs
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
