namespace Monster.Middle.Processes.Sync.Misc
{
    public static class WeightCalc
    {
        public const string AcumaticaUnitsOfMeasure = "OZ";

        public static decimal ToAcumaticaOunces(this int shopifyGrams)
        {
            return ((decimal)shopifyGrams) * 0.035274m;
        }

        public static int ToShopifyGrams(this double acumaticaWeightOz)
        {
            return (int)(acumaticaWeightOz * 28.3495d);
        }
    }
}

