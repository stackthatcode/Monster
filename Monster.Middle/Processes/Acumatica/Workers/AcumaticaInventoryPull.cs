using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Distribution;
using Monster.Acumatica.Config;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaInventoryPull
    {
        private readonly DistributionClient _inventoryClient;
        private readonly AcumaticaInventoryRepository _inventoryRepository;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly AcumaticaTimeZoneService _instanceTimeZoneService;
        private readonly ExecutionLogService _executionLogService;

        public int PageSize = 50;

        public AcumaticaInventoryPull(
                    DistributionClient inventoryClient, 
                    AcumaticaInventoryRepository inventoryRepository,
                    AcumaticaBatchRepository batchStateRepository,
                    AcumaticaTimeZoneService instanceTimeZoneService,
                    AcumaticaHttpConfig acumaticaHttpConfig,
                    ExecutionLogService executionLogService)
        {
            _inventoryClient = inventoryClient;
            _inventoryRepository = inventoryRepository;
            _batchStateRepository = batchStateRepository;
            _executionLogService = executionLogService;
            _instanceTimeZoneService = instanceTimeZoneService;

            PageSize = acumaticaHttpConfig.PageSize;
        }


        public void RunAutomatic()
        {
            var batchState = _batchStateRepository.Retrieve();
            _executionLogService.InsertExecutionLog("Pulling Stock Items from Acumatica");

            if (batchState.AcumaticaStockItemPullEnd.HasValue)
            {
                RunUpdated();
            }
            else
            {
                RunAll();
            }
        }

        public void RunAll()
        {
            var startOfRun = DateTime.UtcNow;
            var page = 1;
            
            while (true)
            {
                var json = _inventoryClient.RetrieveStockItems(page: page, pageSize: PageSize);

                var stockItems = json.DeserializeFromJson<List<StockItem>>();

                UpsertStockItemToPersist(stockItems);

                if (stockItems.Count == 0)
                {
                    break;
                }

                page++;
            }

            var batchStateEnd = startOfRun.AddAcumaticaBatchFudge();
            _batchStateRepository.UpdateProductsEnd(batchStateEnd);
        }
        
        public void RunUpdated()
        {
            var startOfRun = DateTime.UtcNow;

            var batchState = _batchStateRepository.Retrieve();
            if (!batchState.AcumaticaStockItemPullEnd.HasValue)
            {
                throw new Exception(
                    "AcumaticaStockItemPullEnd is null - run Acumatica Baseline Pull first");
            }

            var updateMinUtc = batchState.AcumaticaStockItemPullEnd;
            var updateMin = _instanceTimeZoneService.ToAcumaticaTimeZone(updateMinUtc.Value);

            var itemWarehouses = _inventoryClient.RetrieveItemWarehouses();

            var json = _inventoryClient.RetrieveStockItems(updateMin);
            var stockItems = json.DeserializeFromJson<List<StockItem>>();

            UpsertStockItemToPersist(stockItems);

            _batchStateRepository.UpdateProductsEnd(startOfRun);
        }

        public void UpsertStockItemToPersist(List<StockItem> items)
        {
            foreach (var item in items)
            {
                var existingData = _inventoryRepository.RetreiveStockItem(item.InventoryID.value);

                if (existingData == null)
                {
                    var newData = new AcumaticaStockItem();
                    newData.ItemId = item.InventoryID.value;
                    newData.AcumaticaJson = item.SerializeToJson();
                    newData.AcumaticaDescription = item.Description.value;
                    newData.IsPriceSynced = false;
                    newData.DateCreated = DateTime.UtcNow;
                    newData.LastUpdated = DateTime.UtcNow;

                    _inventoryRepository.InsertStockItems(newData);
                }
                else
                {
                    existingData.AcumaticaJson = item.SerializeToJson();
                    existingData.AcumaticaDescription = item.Description.value;
                    existingData.LastUpdated = DateTime.UtcNow;
                    existingData.IsPriceSynced = false;

                    _inventoryRepository.SaveChanges();
                }

                UpsertWarehouseDetails(item);
            }
        }

        public void UpsertWarehouseDetails(StockItem stockItem)
        {
            var monsterStockItem = 
                    _inventoryRepository.RetreiveStockItem(stockItem.InventoryID.value);

            var stockItemMonsterId = monsterStockItem.MonsterId;

            var existingDetails = 
                    _inventoryRepository.RetrieveWarehouseDetails(stockItemMonsterId);

            var warehouses = _inventoryRepository.RetrieveWarehouses();

            foreach (var acumaticaDetail in stockItem.WarehouseDetails)
            {
                var acumaticaWarehouseId = acumaticaDetail.WarehouseID.value;
                var monsterWarehouse
                    = warehouses.First(x => x.AcumaticaWarehouseId == acumaticaWarehouseId);

                var existingDetail 
                    = existingDetails
                        .FirstOrDefault(x => x.AcumaticaWarehouseId == acumaticaWarehouseId);

                if (existingDetail == null)
                {
                    var newDetail = new AcumaticaWarehouseDetail();
                    newDetail.ParentMonsterId = monsterStockItem.MonsterId;
                    newDetail.AcumaticaJson = acumaticaDetail.SerializeToJson();
                    newDetail.AcumaticaWarehouseId = acumaticaDetail.WarehouseID.value;
                    newDetail.AcumaticaQtyOnHand = acumaticaDetail.QtyOnHand.value;
                    newDetail.WarehouseMonsterId = monsterWarehouse.Id;
                    newDetail.IsInventorySynced = false;
                    newDetail.DateCreated = DateTime.UtcNow;
                    newDetail.LastUpdated = DateTime.UtcNow;

                    _inventoryRepository.InsertWarehouseDetails(newDetail);
                }
                else
                {
                    existingDetail.AcumaticaQtyOnHand = acumaticaDetail.QtyOnHand.value;
                    existingDetail.AcumaticaJson = acumaticaDetail.SerializeToJson();
                    existingDetail.IsInventorySynced = false;
                    existingDetail.LastUpdated = DateTime.UtcNow;
                    _inventoryRepository.SaveChanges();
                }
            }
        }
    }
}

