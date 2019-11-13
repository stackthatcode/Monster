﻿using System.Text.RegularExpressions;
using Monster.Middle.Persist.Instance;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Sync.Model.Misc
{
    public class Canonizers
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

    public static class CanonizerExtensions
    {
        public static string StandardizedSku(this ShopifyVariant input)
        {
            return Canonizers.Sku(input.ShopifySku);
        }

        public static string StandardizedName(this ShopifyLocation input)
        {
            return Canonizers.LocationName(input.ShopifyLocationName);
        }
    }
}