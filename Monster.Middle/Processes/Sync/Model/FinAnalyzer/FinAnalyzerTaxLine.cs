using System;
using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.Processes.Sync.Model.FinAnalyzer
{
    public class FinAnalyzerTaxLine
    {
        public string Name { get; private set; }
        public decimal Rate { get; private set; }

        public FinAnalyzerTaxLine(string name, decimal rate)
        {
            Name = name;
            Rate = rate;
        }
    }

    public static class TransferTaxLineFunctions
    {
        public static decimal CalculateTaxes(this IEnumerable<FinAnalyzerTaxLine> taxLines, decimal price)
        {
            return taxLines.Sum(x => Math.Round(price * x.Rate, 2));
        }
    }
}
