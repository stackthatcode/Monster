using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Distribution;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Inventory
{
    public class AcumaticaProductWorker
    {
        private readonly InventoryClient _inventoryClient;
        private readonly IPushLogger _logger;

        public AcumaticaProductWorker(
                    InventoryClient inventoryClient, IPushLogger logger)
        {
            _inventoryClient = inventoryClient;
            _logger = logger;
        }

        public void PullStockItems()
        {
            var json = _inventoryClient.RetreiveStockItems();

            var stockItems = json.DeserializeFromJson<List<StockItem>>();

            

        }
    }
}

