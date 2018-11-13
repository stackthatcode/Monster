using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Sync
{
    public class SyncInventoryRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public SyncInventoryRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }



        // Warehouses & Locations
        //
        public void InsertInventoryReceiptSync(
                UsrShopifyInventoryLevel level,
                UsrAcumaticaInventoryReceipt receipt)
        {
            var sync = new UsrInventoryReceiptSync();
            sync.UsrShopifyInventoryLevel = level;
            sync.UsrAcumaticaInventoryReceipt = receipt;
            Entities.UsrInventoryReceiptSyncs.Add(sync);
            Entities.SaveChanges();
        }

        public void InsertWarehouseSync(
                UsrShopifyLocation location,
                UsrAcumaticaWarehouse warehouse,
                bool isNameMismatched = false)
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
        public UsrShopifyVariant 
                    RetrieveVariant(long shopifyVariantId, string sku)
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
                .Include(x => x.UsrShopAcuItemSyncs.Select(y => y.UsrShopifyVariant))
                .FirstOrDefault(x => x.ItemId == itemId);
        }

        public List<UsrShopifyVariant> RetrieveVariants(bool? isMatched = null)
        {
            var output =
                Entities
                    .UsrShopifyVariants
                    .Include(x => x.UsrShopifyProduct)
                    .Include(x => x.UsrShopAcuItemSyncs)
                    .Include(x => x.UsrShopAcuItemSyncs.Select(y => y.UsrAcumaticaStockItem));

            if (isMatched.HasValue)
            {
                output = output.Where(x => x.UsrShopAcuItemSyncs.Any());
            }

            return output.ToList();
        }

        public List<UsrShopifyVariant> 
                        RetrieveVariantsWithStockItems(string sku)
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
                UsrShopifyVariant variant, UsrAcumaticaStockItem stockItem)
        {
            var sync = new UsrShopAcuItemSync();
            sync.UsrShopifyVariant = variant;
            sync.UsrAcumaticaStockItem = stockItem;
            sync.DateCreated = DateTime.UtcNow;
            sync.LastUpdated = DateTime.UtcNow;
            Entities.UsrShopAcuItemSyncs.Add(sync);
            Entities.SaveChanges();
        }



        // Inventory and Warehouse Details
        //
        public List<UsrShopifyInventoryLevel> RetrieveInventoryLevelsNotSynced()
        {
            return Entities
                    .UsrShopifyInventoryLevels
                    .Include(x => x.UsrShopifyVariant)
                    .Include(x => x.UsrShopifyVariant.UsrShopAcuItemSyncs)
                    .Include(x => x.UsrShopifyLocation)
                    .Where(x => !x.UsrInventoryReceiptSyncs.Any()
                                && x.ShopifyAvailableQuantity > 0)
                    .ToList();
        }
        
        public List<UsrAcumaticaWarehouseDetail> RetrieveWarehouseDetailsNotSynced()
        {
            return Entities
                .UsrAcumaticaWarehouseDetails
                .Include(x => x.UsrAcumaticaStockItem)
                .Include(x => x.UsrAcumaticaStockItem.UsrShopAcuItemSyncs)
                .Where(x => x.IsShopifySynced.HasValue && x.IsShopifySynced.Value == false)
                .ToList();
        }



        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
