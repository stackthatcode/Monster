using System.Collections.Generic;
using PX.TaxProvider;

namespace Monster.TaxProvider.Calc
{
    public class ProviderTaxCalcResult
    {
        public string TaxID { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal Rate { get; set; }
        public decimal TaxAmount { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Failed => ErrorMessages.Count > 0;

        public ProviderTaxCalcResult()
        {
            ErrorMessages = new List<string>();
        }

        public GetTaxResult ToGetTaxResult()
        {
            var output = new GetTaxResult();
            var taxLines = new List<TaxLine>();

            var details = new TaxDetail()
            {
                TaxName = TaxID,
                Rate = 0.00m,
                TaxAmount = TaxAmount,
                TaxableAmount = TaxableAmount,
                TaxCalculationLevel = TaxCalculationLevel.CalcOnItemAmt,
            };

            taxLines.Add(new TaxLine()
            {
                Index = 1,
                Rate = 0.00m,
                TaxAmount = TaxAmount,
                TaxableAmount = TaxableAmount,
                TaxDetails = new[] { details },
            });

            output.TaxLines = taxLines.ToArray();
            output.TotalAmount = TaxableAmount;
            output.TotalTaxAmount = TaxAmount;
            output.TaxSummary = new TaxDetail[] { details };
            output.IsSuccess = true;

            return output;
        }

        public static ProviderTaxCalcResult Make(
                string taxId, decimal taxableAmount, decimal taxAmount, decimal rate)
        {
            var output = new ProviderTaxCalcResult();
            output.TaxID = taxId;
            output.Rate = rate;
            output.TaxableAmount = taxableAmount;
            output.TaxAmount = taxAmount;
            return output;
        }
    }
}
