using System.Collections.Generic;
using System.Linq;
using Monster.TaxProvider.Acumatica;
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

            var details = new TaxDetail()
            {
                TaxName = AcumaticaTaxIdentifiers.LineItemsTaxID,
                Rate = 0.00m,
                TaxableAmount = TaxableAmount,
                TaxAmount = TaxAmount,
                TaxCalculationLevel = TaxCalculationLevel.CalcOnItemAmt,
            };

            taxLines.Add(new TaxLine()
            {
                Index = index,
                Rate = 0.00m,
                TaxableAmount = TaxableAmount,
                TaxAmount = TaxAmount,
                TaxDetails = new[] { details },
            });

            output.TaxLines = taxLines.ToArray();
            output.TotalAmount = TaxableAmount;
            output.TotalTaxAmount = TaxAmount;

            output.TaxSummary = new TaxDetail[] { details };
            output.IsSuccess = true;

            return output;
        }

    }
}

