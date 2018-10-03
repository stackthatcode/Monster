using System.Collections.Generic;
using System.Linq;
using Push.Shopify.Api.Inventory;

namespace Monster.Middle.Persist.Multitenant.Extensions
{
    public static class InventoryExtensions
    {
        public static UsrShopifyLocation Find(
                this IList<UsrShopifyLocation> persistLocations,
                Location location)
        {
            return persistLocations
                .FirstOrDefault(x => x.ShopifyLocationId == location.id);
        }

    }
}
