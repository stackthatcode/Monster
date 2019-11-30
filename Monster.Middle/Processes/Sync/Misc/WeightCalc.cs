namespace Monster.Middle.Processes.Sync.Misc
{
    public static class WeightCalc
    {
        public const string AcumaticaUnitsOfMeasure = "OZ";

        public static decimal ShopifyGramsToAcumaticaOz(this int shopifyGrams)
        {
            return ((decimal)shopifyGrams) * 0.035274m;
        }
    }
}
