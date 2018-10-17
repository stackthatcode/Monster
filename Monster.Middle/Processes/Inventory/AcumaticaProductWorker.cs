using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Inventory
{
    public class AcumaticaProductWorker
    {
        private readonly InventoryClient _inventoryClient;
        private readonly InventoryRepository _inventoryRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaProductWorker(
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

        // TODO - log run start and end times

        public void BaselinePullStockItems()
        {
            var json = _inventoryClient.RetreiveStockItems();
            var stockItems = json.DeserializeFromJson<List<StockItem>>();
            UpsertStockItems(stockItems);

            var maxProductDate = 
                _inventoryRepository
                    .RetrieveAcumaticaStockItemsMaxUpdatedDate();

            var batchStateEnd 
                = maxProductDate 
                    ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            _batchStateRepository.UpdateAcumaticaProductsEnd(batchStateEnd);
        }
        
        public void DiffPullStockItems()
        {
            var batchState = _batchStateRepository.RetrieveBatchState();
            if (!batchState.AcumaticaProductsEndDate.HasValue)
            {
                throw new Exception(
                    "AcumaticaProductsEndDate is null - run Acumatica Baseline Pull first");
            }

            var productUpdateMin = batchState.AcumaticaProductsEndDate;
            var pullRunStartTime = DateTime.UtcNow;

            var json = _inventoryClient.RetreiveStockItems(productUpdateMin);
            var stockItems = json.DeserializeFromJson<List<StockItem>>();

            UpsertStockItems(stockItems);

            _batchStateRepository.UpdateAcumaticaProductsEnd(pullRunStartTime);
        }

        public void UpsertStockItems(List<StockItem> items)
        {
            foreach (var item in items)
            {
                var existingData
                    = _inventoryRepository.RetreiveAcumaticaStockItems(item.id);

                if (existingData == null)
                {
                    var newData = new UsrAcumaticaStockItem()
                    {
                        ItemId = item.id,
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
            }
        }
    }
}

