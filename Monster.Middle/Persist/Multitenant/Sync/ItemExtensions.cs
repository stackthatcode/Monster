using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Sync
{
    public static class ItemExtensions
    {
        public static UsrAcumaticaStockItem 
                    MatchedStockItem(this UsrShopifyVariant input)
        {
            return input
                .UsrShopAcuItemSyncs?.First().UsrAcumaticaStockItem;
        }

        public static UsrShopifyVariant
                    MatchedVariant(this UsrAcumaticaStockItem input)
        {
            return input
                .UsrShopAcuItemSyncs?.First().UsrShopifyVariant;
        }
        
    }
}
