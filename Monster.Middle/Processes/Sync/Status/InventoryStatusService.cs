using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;

namespace Monster.Middle.Processes.Sync.Status
{
    public class InventoryStatusService
    {
        private readonly SyncInventoryRepository _inventoryRepository;

        public InventoryStatusService(SyncInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public InventorySyncStatus StockItemStatus(string itemId)
        {
            var stockItem = _inventoryRepository.RetrieveStockItem(itemId);
            return InventorySyncStatus.Make(stockItem);
        }
    }
}

