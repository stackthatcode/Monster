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
        private readonly InventoryRepository _repository;
        private readonly IPushLogger _logger;

        public AcumaticaProductWorker(
                    InventoryClient inventoryClient, 
                    InventoryRepository repository,
                    IPushLogger logger)
        {
            _inventoryClient = inventoryClient;
            _repository = repository;
            _logger = logger;
        }

        public void PullStockItems()
        {
            var json = _inventoryClient.RetreiveStockItems();
            var stockItems = json.DeserializeFromJson<List<StockItem>>();

            foreach (var item in stockItems)
            {
                var existingData 
                        = _repository.RetreiveAcumaticaStockItems(item.id);

                if (existingData == null)
                {
                    var newData = new UsrAcumaticaStockItem()
                    {
                        ItemId = item.id,
                        AcumaticaJson = item.SerializeToJson(),
                        DateCreated = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                    };

                    _repository.InsertAcumaticaStockItems(newData);
                }
                else
                {
                    existingData.AcumaticaJson = item.SerializeToJson();
                    existingData.LastUpdated = DateTime.UtcNow;

                    _repository.SaveChanges();
                }
            }
        }


    }
}

