using System.Collections.Generic;
using System.Linq;
using PX.TaxProvider;

namespace Monster.TaxProvider.Calc
{
    public class ProviderTaxCalcResult
    {
        public List<ProviderTaxCalcResultItem> Items { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Failed => ErrorMessages.Count > 0;

        public decimal TaxableAmount => Items.Sum(x => x.TaxableAmount);
        public decimal TaxAmount => Items.Sum(x => x.TaxAmount);


        public ProviderTaxCalcResult()
        {
            ErrorMessages = new List<string>();
            Items = new List<ProviderTaxCalcResultItem>();
        }

        public GetTaxResult ToGetTaxResult()
        {
            var output = new GetTaxResult();
            var taxLines = new List<TaxLine>();

            var index = 1;

            foreach (var line in Items)
            {
                var details = new TaxDetail()
                {
                    TaxName = line.TaxID,
                    Rate = 0.00m,
                    TaxAmount = line.TaxAmount,
                    TaxableAmount = line.TaxableAmount,
                    TaxCalculationLevel = TaxCalculationLevel.CalcOnItemAmt,
                };

                taxLines.Add(new TaxLine()
                {
                    Index = index,
                    Rate = 0.00m,
                    TaxAmount = line.TaxAmount,
                    TaxableAmount = line.TaxableAmount,
                    TaxDetails = new[] { details },
                });

                index++;
            }

            output.TaxLines = taxLines.ToArray();
            output.TotalAmount = TaxableAmount;
            output.TotalTaxAmount = TaxAmount;

            //output.TaxSummary = new TaxDetail[] { details };
            output.TaxSummary = new TaxDetail[] {  };
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
