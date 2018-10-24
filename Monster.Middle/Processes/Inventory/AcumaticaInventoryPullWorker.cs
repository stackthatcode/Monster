using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Inventory
{
    public class AcumaticaInventoryPullWorker
    {
        private readonly InventoryClient _inventoryClient;
        private readonly InventoryRepository _inventoryRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaInventoryPullWorker(
                    InventoryClient inventoryClient, 
                    InventoryRepository inventoryRepository,
                    BatchStateRepository batchStateRepository,
                    IPushLogger logger)
        {
            _inventoryClient = inventoryClient;
            _inventoryRepository = inventoryRepository;
            _logger = logger;
            _batchStateRepository = batchStateRepository;
        }

        //
        // TODO - log run start and end times
        //
        public void BaselinePull()
        {
            var json = _inventoryClient.RetreiveStockItems();
            var stockItems = json.DeserializeFromJson<List<StockItem>>();
            UpsertStockItemToPersist(stockItems);

            var maxProductDate = 
                _inventoryRepository
                    .RetrieveAcumaticaStockItemsMaxUpdatedDate();

            var batchStateEnd 
                = maxProductDate 
                    ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            _batchStateRepository
                    .UpdateAcumaticaProductsEnd(batchStateEnd);
        }
        
        public void DifferentialPull()
        {
            var batchState = _batchStateRepository.RetrieveBatchState();
            if (!batchState.AcumaticaProductsPullEnd.HasValue)
            {
                throw new Exception(
                    "AcumaticaProductsPullEnd is null - run Acumatica Baseline Pull first");
            }

            var productUpdateMin = batchState.AcumaticaProductsPullEnd;
            var pullRunStartTime = DateTime.UtcNow;

            var json = _inventoryClient.RetreiveStockItems(productUpdateMin);
            var stockItems = json.DeserializeFromJson<List<StockItem>>();

            UpsertStockItemToPersist(stockItems);

            _batchStateRepository.UpdateAcumaticaProductsEnd(pullRunStartTime);
        }

        public void UpsertStockItemToPersist(List<StockItem> items)
        {
            foreach (var item in items)
            {
                var existingData
                    = _inventoryRepository
                        .RetreiveAcumaticaStockItem(item.InventoryID.value);

                if (existingData == null)
                {
                    var newData = new UsrAcumaticaStockItem()
                    {
                        ItemId = item.InventoryID.value,
                        AcumaticaJson = item.SerializeToJson(),
                        DateCreated = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                    };

                    _inventoryRepository.InsertAcumaticaStockItems(newData);
                }
                else
                {
                    existingData.AcumaticaJson = item.SerializeToJson();
                    existingData.LastUpdated = DateTime.UtcNow;

                    _inventoryRepository.SaveChanges();
                }

                UpsertWarehouseDetails(item);
            }
        }

        public void UpsertWarehouseDetails(StockItem stockItem)
        {
            var monsterStockItem = 
                    _inventoryRepository
                        .RetreiveAcumaticaStockItem(stockItem.InventoryID.value);

            var stockItemMonsterId = monsterStockItem.MonsterId;

            var existingDetails = 
                _inventoryRepository
                    .RetrieveAcumaticaWarehouseDetails(stockItemMonsterId);

            foreach (var acumaticaDetail in stockItem.WarehouseDetails)
            {
                var acumaticaWarehouseId = acumaticaDetail.WarehouseID.value;
                var existingDetail
                    = existingDetails.FirstOrDefault(
                        x => x.AcumaticaWarehouseId == acumaticaWarehouseId);

                if (existingDetail == null)
                {
                    var newDetail = new UsrAcumaticaWarehouseDetail();
                    newDetail.ParentMonsterId = monsterStockItem.MonsterId;
                    newDetail.AcumaticaJson = acumaticaDetail.SerializeToJson();
                    newDetail.AcumaticaWarehouseId = acumaticaDetail.WarehouseID.value;
                    newDetail.AcumaticaQtyOnHand = acumaticaDetail.QtyOnHand.value;
                    newDetail.DateCreated = DateTime.UtcNow;
                    newDetail.LastUpdated = DateTime.UtcNow;

                    _inventoryRepository.InsertAcumaticaWarehouseDetails(newDetail);
                }
                else
                {
                    existingDetail.AcumaticaQtyOnHand = acumaticaDetail.QtyOnHand.value;
                    existingDetail.AcumaticaJson = acumaticaDetail.SerializeToJson();
                    existingDetail.LastUpdated = DateTime.UtcNow;
                    _inventoryRepository.SaveChanges();
                }
            }
        }
    }
}

