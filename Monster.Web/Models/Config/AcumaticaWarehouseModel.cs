﻿using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Extensions;

namespace Monster.Web.Models.Config
{
    public class AcumaticaWarehouseModel
    {
        public string AcumaticaWarehouseId { get; set; }
        public long? ShopifyLocationId { get; set; }

        public static AcumaticaWarehouseModel Make(UsrAcumaticaWarehouse warehouse)
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