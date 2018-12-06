﻿using System;
using System.Linq;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Processes.Sync.Extensions
{
    public static class WarehouseExtensions
    {
        public static 
                string AcumaticaStockItemId(this UsrShopifyVariant input)
        {
            if (input.UsrShopAcuItemSyncs.Count == 0)
            {
                throw new Exception("No synchronized Acumatica Stock Items");
            }

            return input
                    .UsrShopAcuItemSyncs
                    .First()
                    .UsrAcumaticaStockItem.ItemId;
        }

        public static string AcumaticaWarehouseId(
                this UsrShopifyLocation input)
        {
            if (input.UsrShopAcuWarehouseSyncs.Count == 0)
            {
                throw new Exception("Inventory Level not assigned to Location");
            }

            return input
                .UsrShopAcuWarehouseSyncs
                .First()
                .UsrAcumaticaWarehouse
                .AcumaticaWarehouseId;
        }

    }
}
