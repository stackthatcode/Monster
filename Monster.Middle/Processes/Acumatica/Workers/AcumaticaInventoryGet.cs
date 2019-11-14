using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Distribution;
using Monster.Acumatica.Config;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaInventoryGet
    {
        private readonly DistributionClient _inventoryClient;
        private readonly AcumaticaInventoryRepository _inventoryRepository;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly AcumaticaTimeZoneService _instanceTimeZoneService;
        private readonly AcumaticaHttpConfig _config;
        private readonly ExecutionLogService _executionLogService;
        private readonly SettingsRepository _settingsRepository;

        public AcumaticaInventoryGet(
                    DistributionClient inventoryClient, 
                    AcumaticaInventoryRepository inventoryRepository,
                    AcumaticaBatchRepository batchStateRepository,
                    AcumaticaTimeZoneService instanceTimeZoneService,
                    AcumaticaHttpConfig config,
                    ExecutionLogService executionLogService, 
                    SettingsRepository settingsRepository)
        {
            _inventoryClient = inventoryClient;
            _inventoryRepository = inventoryRepository;
            _batchStateRepository = batchStateRepository;
            _executionLogService = executionLogService;
            _settingsRepository = settingsRepository;
            _instanceTimeZoneService = instanceTimeZoneService;
            _config = config;
        }


        public void RunAutomatic()
        {
            var batchState = _batchStateRepository.Retrieve();
            _executionLogService.Log("Refreshing Stock Items from Acumatica");

            if (batchState.AcumaticaStockItemGetEnd.HasValue)
            {
                Run(batchState.AcumaticaStockItemGetEnd);
            }
            else
            {
                Run();
            }
        }

        public void Run(DateTime? lastModifiedMinUtc = null)
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
                        page: page, pageSize: _config.PageSize, lastModified: lastModifiedAcuTime);

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
        

        public void UpsertStockItemToPersist(List<StockItem> items)
        {
            var settings = _settingsRepository.RetrieveSettings();

            foreach (var item in items)
            {
                var existingData = _inventoryRepository.RetreiveStockItem(item.InventoryID.value);

                if (existingData == null)
                {
                    var newData = new AcumaticaStockItem();
                    newData.ItemId = item.InventoryID.value;
                    newData.AcumaticaJson = item.SerializeToJson();
                    newData.AcumaticaDescription = item.Description.value;
                    newData.AcumaticaTaxCategory = item.TaxCategory.value;

                    newData.IsPriceSynced = false;
                    newData.DateCreated = DateTime.UtcNow;
                    newData.LastUpdated = DateTime.UtcNow;

                    _inventoryRepository.InsertStockItems(newData);
                }
                else
                {
                    existingData.AcumaticaJson = item.SerializeToJson();
                    existingData.AcumaticaTaxCategory = item.TaxCategory.value;
                    existingData.AcumaticaDescription = item.Description.value;

                    existingData.LastUpdated = DateTime.UtcNow;
                    existingData.IsPriceSynced = false;

                    _inventoryRepository.SaveChanges();
                }

                UpsertWarehouseDetails(item);
            }
        }

        public bool? ComputeIsTaxable(MonsterSetting settings, string taxCategory)
        {
            if (taxCategory == settings.AcumaticaTaxableCategory)
            {
                return true;
            }

            if (taxCategory == settings.AcumaticaTaxExemptCategory)
            {
                return false;
            }

            return null;
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

