using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Acumatica.Persist
{
    public class AcumaticaInventoryRepository
    {
        private readonly ProcessPersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public AcumaticaInventoryRepository(ProcessPersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        public DbContextTransaction BeginTransaction()
        {
            return _dataContext.Entities.Database.BeginTransaction();
        }


        public void InsertWarehouse(AcumaticaWarehouse warehouse)
        {
            Entities.AcumaticaWarehouses.Add(warehouse);
            Entities.SaveChanges();
        }

        public List<AcumaticaWarehouse> RetrieveWarehouses()
        {
            return Entities.AcumaticaWarehouses.ToList();
        }

        public AcumaticaWarehouse RetrieveWarehouse(string warehouseId)
        {
            return Entities.AcumaticaWarehouses.FirstOrDefault(x => x.AcumaticaWarehouseId == warehouseId);
        }

        // Acumatica persistence
        //
        public AcumaticaStockItem RetreiveStockItem(string itemId)
        {
            return Entities
                .AcumaticaStockItems
                .Include(x => x.ShopifyVariant)
                .FirstOrDefault(x => x.ItemId == itemId);
        }

        public AcumaticaStockItem RetreiveStockItem(long monsterId)
        {
            return Entities
                    .AcumaticaStockItems
                    .FirstOrDefault(x => x.MonsterId == monsterId);
        }

        public AcumaticaInventory RetrieveInventory(string itemId, string warehouseId)
        {
            return Entities
                .AcumaticaInventories
                .FirstOrDefault(x => x.AcumaticaStockItem.ItemId == itemId
                                     && x.AcumaticaWarehouse.AcumaticaWarehouseId == warehouseId);
        }

        public void InsertInventory(AcumaticaInventory inventory)
        {
            Entities.AcumaticaInventories.Add(inventory);
            Entities.SaveChanges();
        }


        public void InsertStockItems(AcumaticaStockItem item)
        {
            Entities.AcumaticaStockItems.Add(item);
            Entities.SaveChanges();
        }

        public void InsertInventoryReceipt(AcumaticaInventoryReceipt receipt)
        {
            Entities.AcumaticaInventoryReceipts.Add(receipt);
            Entities.SaveChanges();
        }


        public List<AcumaticaInventoryReceipt>
                        RetrieveUnreleasedInventoryReceipts()
        {
            return Entities
                    .AcumaticaInventoryReceipts
                    .Where(x => x.IsReleased == false)
                    .ToList();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
