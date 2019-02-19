namespace Monster.Middle.Processes.Sync.Inventory.Model
{
    public class VariantAndStockItemDto
    {
        public long ShopifyProductId { get; set; }
        public long ShopifyVariantId { get; set; }
        public string ShopifySku { get; set; }
        public string ItemId { get; set; }
    }
}
