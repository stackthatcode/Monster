using System.Collections.Generic;
using System.Linq;
using Monster.TaxProvider.Acumatica;
using PX.TaxProvider;

namespace Monster.TaxProvider.Calc
{
    public class CalcResult
    {
        public List<CalcResultTaxLine> TaxLines { get; set; }
        public List<string> ErrorMessages { get; set; }

        public bool Empty => TaxLines.Count == 0;
        public bool Failed => ErrorMessages.Count > 0;

        public decimal TotalTaxableAmount => TaxLines.Sum(x => x.TaxableAmount);
        public decimal TotalTaxAmount => TaxLines.Sum(x => x.TaxAmount);


        public CalcResult()
        {
            TaxLines = new List<CalcResultTaxLine>();
            ErrorMessages = new List<string>();
        }

        public void AddTaxLine(string desc, decimal rate, decimal taxableAmount, decimal taxAmount)
        {
            var taxLine = new CalcResultTaxLine(desc, rate, taxableAmount, taxAmount);
            this.TaxLines.Add(taxLine);
        }

        public void AddError(string message)
        {
            this.ErrorMessages.Add(message);
        }

        public GetTaxResult ToGetTaxResult(string taxId)
        {
            var output = new GetTaxResult();
            var taxLines = new List<TaxLine>();
            TaxDetail details = null;

            if (!Empty)
            {
                details = new TaxDetail()
                {
                    TaxName = taxId,
                    Rate = 0.00m,
                    TaxableAmount = TotalTaxableAmount,
                    TaxAmount = TotalTaxAmount,
                    TaxCalculationLevel = TaxCalculationLevel.CalcOnItemAmt,
                };

                taxLines.Add(new TaxLine()
                {
                    Index = 1,
                    Rate = 0.00m,
                    TaxableAmount = TotalTaxableAmount,
                    TaxAmount = TotalTaxAmount,
                    TaxDetails = new[] {details},
                });

                output.TaxSummary = new TaxDetail[] { details };
                output.TaxLines = taxLines.ToArray();
            }
            else
            {
                output.TaxSummary = new TaxDetail[] {};
                output.TaxLines = new TaxLine[] {};
            }

            output.TotalAmount = TotalTaxableAmount;
            output.TotalTaxAmount = TotalTaxAmount;
            output.IsSuccess = true;

            return output;
        }
    }
}

