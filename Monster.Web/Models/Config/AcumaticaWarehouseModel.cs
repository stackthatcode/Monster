using Monster.Middle;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Persist.Matching;

namespace Monster.Web.Models.Config
{
    public class AcumaticaWarehouseModel
    {
        public string AcumaticaWarehouseId { get; set; }
        public long? ShopifyLocationId { get; set; }

        public static AcumaticaWarehouseModel Make(AcumaticaWarehouse warehouse)
        {
            var output = new AcumaticaWarehouseModel();
            output.AcumaticaWarehouseId = warehouse.AcumaticaWarehouseId;

            if (warehouse.HasMatch())
            {
                output.ShopifyLocationId = warehouse.MatchedLocation().ShopifyLocationId;
            }

            return output;
        }
    }
}
