﻿using System.Text.RegularExpressions;
using Monster.Middle.Persist.Multitenant;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Sync.Inventory.Model
{
    public class Standards
    {
        public static string StockItemTitle(Product product, Variant variant)
        {
            var output = product.title + " - ";
            output += 
                variant.title != "Default Title"
                    ? variant.title 
                    : variant.sku;
                
            return output;
        }

        public static string Sku(string sku)
        {
            return sku.ToUpper();
        }

        public static string LocationName(string name)
        {
            var output = name.ToUpper();
            var rgx = new Regex("[^a-zA-Z0-9]");
            output = rgx.Replace(output, "");
            return output;
        }
    }

    public static class StandardizeExtensions
    {
        public static string 
                    StandardizedSku(this UsrShopifyVariant input)
        {
            return Standards.Sku(input.ShopifySku);
        }

        public static string 
                    StandardizedName(this UsrShopifyLocation input)
        {
            return Standards.LocationName(input.ShopifyLocationName);
        }
    }
}