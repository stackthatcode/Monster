using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Distribution;
using Monster.Acumatica.Config;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaInventoryGet
    {
        private readonly DistributionClient _inventoryClient;
        private readonly AcumaticaInventoryRepository _inventoryRepository;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly AcumaticaTimeZoneService _instanceTimeZoneService;
        private readonly AcumaticaJsonService _acumaticaJsonService;
        private readonly AcumaticaHttpConfig _config;
        private readonly ExecutionLogService _executionLogService;

        public AcumaticaInventoryGet(
                    DistributionClient inventoryClient, 
                    AcumaticaInventoryRepository inventoryRepository,
                    AcumaticaBatchRepository batchStateRepository,
                    AcumaticaTimeZoneService instanceTimeZoneService,
                    AcumaticaJsonService acumaticaJsonService,
                    ExecutionLogService executionLogService,
                    AcumaticaHttpConfig config)
        {
            _inventoryClient = inventoryClient;
            _inventoryRepository = inventoryRepository;
            _batchStateRepository = batchStateRepository;
            _executionLogService = executionLogService;
            _instanceTimeZoneService = instanceTimeZoneService;
            _acumaticaJsonService = acumaticaJsonService;
            _config = config;
        }


        public void RunAutomatic()
        {
            var batchState = _batchStateRepository.Retrieve();
            _executionLogService.Log("Refreshing Stock Items from Acumatica");
            RunStockItems(batchState.AcumaticaStockItemGetEnd);

            _executionLogService.Log("Refreshing Inventory Status from Acumatica");
            RunInventoryStatus(batchState.AcumaticaInventoryStatusGetEnd);
        }

        public void RunStockItems(DateTime? lastModifiedMinUtc = null)
        {
            var startOfRun = DateTime.UtcNow;
            var page = 1;

            var lastModifiedAcuTime
                = lastModifiedMinUtc.HasValue
                    ? _instanceTimeZoneService.ToAcumaticaTimeZone(lastModifiedMinUtc.Value)
                    : (DateTime?) null;

            while (true)
            {
                var json = 
                    _inventoryClient.RetrieveStockItems(
                        page: page, pageSize: _config.PageSize, lastModifiedAcuTz: lastModifiedAcuTime);

                var stockItems = json.DeserializeFromJson<List<StockItem>>();

                UpsertStockItemToPersist(stockItems);

                if (stockItems.Count == 0)
                {
                    break;
                }

                page++;
            }

            var batchStateEnd = startOfRun.AddAcumaticaBatchFudge();
            _batchStateRepository.UpdateStockItemGetEnd(batchStateEnd);
        }
        
        public void UpsertStockItemToPersist(List<StockItem> items)
        {
            foreach (var item in items)
            {
                var existingData = _inventoryRepository.RetreiveStockItem(item.InventoryID.value);

                using (var transaction = _inventoryRepository.BeginTransaction())
                {

                    if (existingData == null)
                    {
                        var newData = new AcumaticaStockItem();
                        newData.ItemId = item.InventoryID.value;
                        newData.AcumaticaDescription = item.Description.value;
                        newData.AcumaticaTaxCategory = item.TaxCategory.value;
                        newData.AcumaticaDefaultPrice = (decimal) item.DefaultPrice.value;
                        newData.AcumaticaLastCost = (decimal) item.LastCost.value;

                        newData.IsVariantSynced = false;
                        newData.DateCreated = DateTime.UtcNow;
                        newData.LastUpdated = DateTime.UtcNow;

                        _executionLogService.Log(LogBuilder.DetectedNewStockItem(newData));
                        _inventoryRepository.InsertStockItems(newData);

                    }
                    else
                    {
                        existingData.AcumaticaDescription = item.Description.value;
                        existingData.AcumaticaTaxCategory = item.TaxCategory.value;
                        existingData.AcumaticaDefaultPrice = (decimal) item.DefaultPrice.value;
                        existingData.AcumaticaLastCost = (decimal) item.LastCost.value;
                        existingData.IsVariantSynced = false;
                        existingData.LastUpdated = DateTime.UtcNow;

                        _executionLogService.Log(LogBuilder.DetectedChangeToStockItem(existingData));
                        _inventoryRepository.SaveChanges();
                    }

                    _acumaticaJsonService.Upsert(
                        AcumaticaJsonType.StockItem, item.InventoryID.value, item.SerializeToJson());
                    
                    transaction.Commit(); 
                }
            }
        }


        public void RunInventoryStatus(DateTime? lastModifiedMinUtc = null)
        {
            var startOfRun = DateTime.UtcNow;
            var page = 1;

            var lastModifiedAcuTime
                = lastModifiedMinUtc.HasValue
                    ? _instanceTimeZoneService.ToAcumaticaTimeZone(lastModifiedMinUtc.Value)
                    : (DateTime?)null;

            while (true)
            {
                var json =
                    _inventoryClient.RetrieveInventoryStatus(
                        page: page, pageSize: _config.PageSize, lastModifiedAcuTz: lastModifiedAcuTime);

                var status = json.DeserializeFromJson<InventoryStatusParent>();

                UpsertInventoryStatus(status);

                if (status.value.Count == 0)
                {
                    break;
                }

                page++;
            }

            var batchStateEnd = startOfRun.AddAcumaticaBatchFudge();
            _batchStateRepository.UpdateInventoryStatusGetEnd(batchStateEnd);
        }


        public void UpsertInventoryStatus(InventoryStatusParent inventoryStatusParent)
        {
            foreach (var entry in inventoryStatusParent.value)
            {
                UpsertInventoryStatus(entry);
            }
        }

        public void UpsertInventoryStatus(InventoryStatus inventoryStatus)
        {
            var stockItemRecord = _inventoryRepository.RetreiveStockItem(inventoryStatus.InventoryID);

            var existing =
                _inventoryRepository
                    .RetrieveInventory(inventoryStatus.InventoryID, inventoryStatus.WarehouseID);

            if (existing == null)
            {
                var warehouse = _inventoryRepository.RetrieveWarehouse(inventoryStatus.WarehouseID);

                var inventory = new AcumaticaInventory();
                inventory.ParentMonsterId = stockItemRecord.MonsterId;
                inventory.AcumaticaWarehouseId = inventoryStatus.WarehouseID;
                inventory.AcumaticaAvailQty = inventoryStatus.QtyAvailable;
                inventory.WarehouseMonsterId = warehouse.Id;
                inventory.IsInventorySynced = false;
                inventory.DateCreated = DateTime.UtcNow;
                inventory.LastUpdated = DateTime.UtcNow;

                _inventoryRepository.InsertInventory(inventory);
            }
            else
            {
                existing.AcumaticaAvailQty = inventoryStatus.QtyAvailable;
                existing.IsInventorySynced = false;
                existing.LastUpdated = DateTime.UtcNow;
                _inventoryRepository.SaveChanges();
            }
        }
    }
}

