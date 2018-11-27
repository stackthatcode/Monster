using System.Linq;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Processes.Sync.Extensions
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
