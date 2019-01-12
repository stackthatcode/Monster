using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaInventoryPull
    {
        private readonly DistributionClient _inventoryClient;
        private readonly AcumaticaInventoryRepository _inventoryRepository;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly InstanceTimeZoneService _instanceTimeZoneService;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaInventoryPull(
                    DistributionClient inventoryClient, 
                    AcumaticaInventoryRepository inventoryRepository,
                    AcumaticaBatchRepository batchStateRepository,
                    InstanceTimeZoneService instanceTimeZoneService,
                    IPushLogger logger)
        {
            _inventoryClient = inventoryClient;
            _inventoryRepository = inventoryRepository;
            _batchStateRepository = batchStateRepository;
            _logger = logger;
            _instanceTimeZoneService = instanceTimeZoneService;
        }


        public void RunAutomatic()
        {
            var batchState = _batchStateRepository.Retrieve();
            if (batchState.AcumaticaProductsPullEnd.HasValue)
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

            var json = _inventoryClient.RetreiveStockItems();
            var stockItems = json.DeserializeFromJson<List<StockItem>>();

            UpsertStockItemToPersist(stockItems);

            var maxProductDate = 
                _inventoryRepository
                    .RetrieveStockItemsMaxUpdatedDate();

            var batchStateEnd
                = (maxProductDate ?? startOfRun).AddAcumaticaBatchFudge();
            
            _batchStateRepository.UpdateProductsEnd(batchStateEnd);
        }
        
        public void RunUpdated()
        {
            var startOfRun = DateTime.UtcNow;

            var batchState = _batchStateRepository.Retrieve();
            if (!batchState.AcumaticaProductsPullEnd.HasValue)
            {
                throw new Exception(
                    "AcumaticaProductsPullEnd is null - run Acumatica Baseline Pull first");
            }

            var updateMinUtc = batchState.AcumaticaProductsPullEnd;
            var updateMin = _instanceTimeZoneService.ToAcumaticaTimeZone(updateMinUtc.Value);

            var json = _inventoryClient.RetreiveStockItems(updateMin);
            var stockItems = json.DeserializeFromJson<List<StockItem>>();

            UpsertStockItemToPersist(stockItems);

            _batchStateRepository.UpdateProductsEnd(startOfRun);
        }

        public void UpsertStockItemToPersist(List<StockItem> items)
        {
            foreach (var item in items)
            {
                var existingData
                    = _inventoryRepository
                        .RetreiveStockItem(item.InventoryID.value);

                if (existingData == null)
                {
                    var newData = new UsrAcumaticaStockItem()
                    {
                        ItemId = item.InventoryID.value,
                        AcumaticaJson = item.SerializeToJson(),
                        DateCreated = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                    };

                    _inventoryRepository.InsertStockItems(newData);
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
                        .RetreiveStockItem(stockItem.InventoryID.value);

            var stockItemMonsterId = monsterStockItem.MonsterId;

            var existingDetails = 
                _inventoryRepository
                    .RetrieveWarehouseDetails(stockItemMonsterId);

            var warehouses = _inventoryRepository.RetrieveWarehouses();

            foreach (var acumaticaDetail in stockItem.WarehouseDetails)
            {
                var acumaticaWarehouseId = acumaticaDetail.WarehouseID.value;
                var monsterWarehouse
                    = warehouses.First(x => x.AcumaticaWarehouseId == acumaticaWarehouseId);

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
                    newDetail.WarehouseMonsterId = monsterWarehouse.Id;
                    newDetail.IsShopifySynced = false;
                    newDetail.DateCreated = DateTime.UtcNow;
                    newDetail.LastUpdated = DateTime.UtcNow;

                    _inventoryRepository.InsertWarehouseDetails(newDetail);
                }
                else
                {
                    existingDetail.AcumaticaQtyOnHand = acumaticaDetail.QtyOnHand.value;
                    existingDetail.AcumaticaJson = acumaticaDetail.SerializeToJson();
                    existingDetail.IsShopifySynced = false;
                    existingDetail.LastUpdated = DateTime.UtcNow;
                    _inventoryRepository.SaveChanges();
                }
            }
        }
    }
}

