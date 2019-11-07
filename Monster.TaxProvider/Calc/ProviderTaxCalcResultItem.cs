using PX.TaxProvider;

namespace Monster.TaxProvider.Calc
{
    public class ProviderTaxCalcResultItem
    {
        public string TaxID { get; private set; }
        public decimal TaxableAmount { get; private set; }
        public decimal Rate { get; private set; }
        public decimal TaxAmount { get; private set; }

        public ProviderTaxCalcResultItem(
                string taxId, decimal taxableAmount, decimal taxAmount, decimal rate)
        {
            TaxID = taxId;
            Rate = rate;
            TaxableAmount = taxableAmount;
            TaxAmount = taxAmount;
        }


        public TaxLine ToTaxDetail()
        {
            var details = new TaxDetail()
            {
                TaxName = TaxID,
                Rate = 0.00m,
                TaxAmount = TaxAmount,
                TaxableAmount = TaxableAmount,
                TaxCalculationLevel = TaxCalculationLevel.CalcOnItemAmt,
            };

            var output = new TaxLine()
            {
                Index = 1,
                Rate = 0.00m,
                TaxAmount = TaxAmount,
                TaxableAmount = TaxableAmount,
                TaxDetails = new[] { details },
            };

            return output;
        }
    }
}
