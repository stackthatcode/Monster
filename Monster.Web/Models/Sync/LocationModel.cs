﻿using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;

namespace Monster.Web.Models.Sync
{
    public class LocationModel
    {
        public string ShopifyLocationName { get; set; }
        public long ShopifyLocationId { get; set; }
        public List<string> WarehouseIds { get; set; }

        public bool MultipleWarehouses => WarehouseIds.Count > 1;

        public static LocationModel Make(ShopifyLocation input)
        {
            var output = new LocationModel();
            output.ShopifyLocationId = input.ShopifyLocationId;
            output.ShopifyLocationName = input.ShopifyLocationName;
            output.WarehouseIds
                = input.ShopAcuWarehouseSyncs
                    .Select(x => x.AcumaticaWarehouse.AcumaticaWarehouseId)
                    .ToList();

            return output;
        }
    }

    public class LocationsModel
    {
        public List<LocationModel> Locations { get; set; }

        public bool AnyMultipleWarehouseMapping => Locations.Any(x => x.MultipleWarehouses);
        

        public static LocationsModel Make(IEnumerable<ShopifyLocation> locations)
        {
            var output = new LocationsModel();
            output.Locations = locations.Select(x => LocationModel.Make(x)).ToList();
            return output;
        }
    }
}