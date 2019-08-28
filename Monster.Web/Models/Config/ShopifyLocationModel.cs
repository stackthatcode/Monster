using Monster.Middle.Persist.Instance;

namespace Monster.Web.Models.Config
{
    public class ShopifyLocationModel
    {
        public long LocationId { get; set; }
        public string LocationName { get; set; }
        public bool IsActive { get; set; }

        public static ShopifyLocationModel Make(ShopifyLocation location)
        {
            return new ShopifyLocationModel()
            {
                LocationId = location.ShopifyLocationId,
                LocationName = "Shopify - " + location.ShopifyLocationName,
                IsActive = location.ShopifyActive,
            };
        }
    }
}
