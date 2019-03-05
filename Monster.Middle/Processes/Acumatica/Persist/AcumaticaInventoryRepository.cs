using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Tenant;

namespace Monster.Middle.Processes.Acumatica.Persist
{
    public class AcumaticaInventoryRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public AcumaticaInventoryRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }


        // Reference data
        static object RefDataLock = new object();
        public UsrAcumaticaRefData RetrieveReferenceData()
        {
            lock (RefDataLock)
            {
                if (!_dataContext.Entities.UsrAcumaticaRefDatas.Any())
                {
                    var newdata = new UsrAcumaticaRefData();
                    _dataContext.Entities.UsrAcumaticaRefDatas.Add(newdata);
                    _dataContext.Entities.SaveChanges();
                }
            }

            return _dataContext.Entities.UsrAcumaticaRefDatas.First();
        }



        public void InsertWarehouse(UsrAcumaticaWarehouse warehouse)
        {
            Entities.UsrAcumaticaWarehouses.Add(warehouse);
            Entities.SaveChanges();
        }

        public IList<UsrAcumaticaWarehouse> RetrieveWarehouses()
        {
            return Entities.UsrAcumaticaWarehouses.ToList();
        }

        // Acumatica persistence
        //
        public UsrAcumaticaStockItem RetreiveStockItem(string itemId)
        {
            return Entities
                .UsrAcumaticaStockItems
                .Include(x => x.UsrShopAcuItemSyncs)
                .FirstOrDefault(x => x.ItemId == itemId);
        }

        public UsrAcumaticaStockItem RetreiveStockItem(long monsterId)
        {
            return Entities
                    .UsrAcumaticaStockItems
                    .FirstOrDefault(x => x.MonsterId == monsterId);
        }

        public List<UsrAcumaticaWarehouseDetail>
                    RetrieveWarehouseDetails(long stockItemMonstedId)
        {
            return Entities
                .UsrAcumaticaWarehouseDetails
                .Where(x => x.ParentMonsterId == stockItemMonstedId)
                .ToList();
        }


        public void InsertWarehouseDetails(UsrAcumaticaWarehouseDetail details)
        {
            Entities.UsrAcumaticaWarehouseDetails.Add(details);
            Entities.SaveChanges();
        }



        public DateTime? RetrieveStockItemsMaxUpdatedDate()
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

        public void InsertStockItems(UsrAcumaticaStockItem item)
        {
            Entities.UsrAcumaticaStockItems.Add(item);
            Entities.SaveChanges();
        }

        public void InsertInventoryReceipt(UsrAcumaticaInventoryReceipt receipt)
        {
            Entities.UsrAcumaticaInventoryReceipts.Add(receipt);
            Entities.SaveChanges();
        }


        public List<UsrAcumaticaInventoryReceipt>
                        RetrieveUnreleasedInventoryReceipts()
        {
            return Entities
                    .UsrAcumaticaInventoryReceipts
                    .Where(x => x.IsReleased == false)
                    .ToList();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
