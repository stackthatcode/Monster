﻿using System;
using System.Collections.Generic;
using Push.Foundation.Utilities.General;

namespace Monster.Middle.Processes.Sync.Inventory.Model
{
    public class WarehouseSyncState
    {
        public IList<string> MatchedWarehouseLocations { get; set; }
        public IList<string> UnmatchedShopifyLocations { get; set; }
        public IList<string> UnmatchedAcumaticaWarehouses { get; set; }
        public IList<string> MismatchedWarehouseLocations { get; set; }

        public string GetSynopsis(string delimiter = null)
        {
            var output = new List<string>();
            output.Add($"{UnmatchedShopifyLocations.Count} unmatched Shopify Location(s)");
            output.Add($"{UnmatchedAcumaticaWarehouses.Count} unmatched Acumatica Warehouse(s)");
            output.Add($"{MismatchedWarehouseLocations.Count} mismatched Warehouse to Location pair(s)");
            return output.ToDelimited(delimiter ?? Environment.NewLine);
        }
        
        public bool OK =>
                UnmatchedShopifyLocations.Count == 0 
                && UnmatchedAcumaticaWarehouses.Count == 0 
                && MismatchedWarehouseLocations.Count == 0;
    }
}