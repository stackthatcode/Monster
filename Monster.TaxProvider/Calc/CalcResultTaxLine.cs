using PX.TaxProvider;

namespace Monster.TaxProvider.Calc
{
    public class CalcResultTaxLine
    {
        public string Desc { get; private set; }
        public decimal Rate { get; private set; }
        public decimal TaxableAmount { get; private set; }
        public decimal TaxAmount { get; private set; }

        public CalcResultTaxLine(string desc, decimal rate, decimal taxableAmount, decimal taxAmount)
        {
            Desc = desc;
            Rate = rate;
            TaxableAmount = taxableAmount;
            TaxAmount = taxAmount;
        }
    }
}
