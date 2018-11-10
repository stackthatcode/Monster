namespace Monster.Middle.Persist.Multitenant.Acumatica
{
    public static class Extensions
    {
        public static bool IsReadyForShipment(this UsrAcumaticaSalesOrder order)
        {
            return order.AcumaticaStatus == "Open";
        }
        
        public static bool IsMatchedToShopify(this UsrAcumaticaStockItem stockItem)
        {
            return stockItem.UsrShopAcuItemSyncs.Count != 0;
        }
    }
}
