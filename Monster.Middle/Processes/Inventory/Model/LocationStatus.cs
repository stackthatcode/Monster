using System;
using System.Collections.Generic;
using Push.Foundation.Utilities.General;

namespace Monster.Middle.Processes.Inventory.Model
{
    public class LocationStatus
    {
        public IList<string> UnmatchedLocations { get; set; }
        public IList<string> UnmatchedWarehouses { get; set; }
        public IList<string> MismatchedWarehouses { get; set; }

        public string GetSynopsis(string delimiter = null)
        {
            var output = new List<string>();
            output.Add($"{UnmatchedLocations.Count} unmatched Shopify Locations");
            output.Add($"{UnmatchedWarehouses.Count} unmatched Acumatica Warehouses");
            output.Add($"{MismatchedWarehouses.Count} unmatched Shopify Locations");
            return output.ToDelimited(delimiter ?? Environment.NewLine);
        }
        
        public bool OK =>
                UnmatchedLocations.Count == 0 
                && UnmatchedWarehouses.Count == 0 
                && MismatchedWarehouses.Count == 0;
    }
}
