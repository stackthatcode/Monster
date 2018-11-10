using System;
using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Acumatica
{
    public static class Extensions
    {
        public static string AcumaticaStockItemId(
            this UsrShopifyInventoryLevel input)
        {
            var stockItems = input
                .UsrShopifyVariant
                .UsrAcumaticaStockItems;

            if (stockItems.Count == 0)
            {
                throw new Exception("No matching Acumatica Stock Items");
            }

            return stockItems.First().ItemId;
        }

        public static string AcumaticaWarehouseId(
            this UsrShopifyInventoryLevel input)
        {
            var location = input.UsrShopifyLocation;
            if (location == null)
            {
                throw new Exception("Inventory Level not assigned to Location");
            }

            var warehouse = location.UsrAcumaticaWarehouses.FirstOrDefault();
            if (warehouse == null)
            {
                throw new Exception("Shopify Location not matched to Acumatica Warehouse");
            }

            return warehouse.AcumaticaWarehouseId;
        }

    }
}

