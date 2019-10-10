using System;
using System.Collections.Generic;
using System.Linq;

namespace Monster.TaxTransfer
{
    public class TransferTaxLine
    {
        public string Name { get; private set; }
        public decimal Rate { get; private set; }

        public TransferTaxLine(string name, decimal rate)
        {
            Name = name;
            Rate = rate;
        }
    }

    public static class TransferTaxLineFunctions
    {
        public static decimal CalculateTaxes(this IEnumerable<TransferTaxLine> taxLines, decimal price)
        {
            return taxLines.Sum(x => Math.Round(price * x.Rate, 2));
        }
    }
}
