using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Acumatica.Persist
{
    public class AcumaticaInventoryRepository
    {
        private readonly InstancePersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public AcumaticaInventoryRepository(InstancePersistContext dataContext)
        {
            _dataContext = dataContext;
        }


        // Reference data
        static object RefDataLock = new object();
        public AcumaticaRefData RetrieveReferenceData()
        {
            lock (RefDataLock)
            {
                if (!_dataContext.Entities.AcumaticaRefDatas.Any())
                {
                    var newdata = new AcumaticaRefData();
                    _dataContext.Entities.AcumaticaRefDatas.Add(newdata);
                    _dataContext.Entities.SaveChanges();
                }
            }

            return _dataContext.Entities.AcumaticaRefDatas.First();
        }



        public void InsertWarehouse(AcumaticaWarehouse warehouse)
        {
            Entities.AcumaticaWarehouses.Add(warehouse);
            Entities.SaveChanges();
        }

        public IList<AcumaticaWarehouse> RetrieveWarehouses()
        {
            return Entities.AcumaticaWarehouses.ToList();
        }

        // Acumatica persistence
        //
        public AcumaticaStockItem RetreiveStockItem(string itemId)
        {
            return Entities
                .AcumaticaStockItems
                .Include(x => x.ShopAcuItemSyncs)
                .FirstOrDefault(x => x.ItemId == itemId);
        }

        public AcumaticaStockItem RetreiveStockItem(long monsterId)
        {
            return Entities
                    .AcumaticaStockItems
                    .FirstOrDefault(x => x.MonsterId == monsterId);
        }

        public List<AcumaticaWarehouseDetail>
                    RetrieveWarehouseDetails(long stockItemMonstedId)
        {
            return Entities
                .AcumaticaWarehouseDetails
                .Where(x => x.ParentMonsterId == stockItemMonstedId)
                .ToList();
        }


        public void InsertWarehouseDetails(AcumaticaWarehouseDetail details)
        {
            Entities.AcumaticaWarehouseDetails.Add(details);
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
