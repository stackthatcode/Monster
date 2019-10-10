using System.Collections.Generic;

namespace Monster.TaxTransfer.Tests
{
    public class TestTaxes
    {
        public static readonly TransferTaxLine ILStateTax
            = new TransferTaxLine("IL State Tax", 0.0625m);
        public static readonly TransferTaxLine MchenryCountyTax 
            = new TransferTaxLine("Mchenry County Tax", 0.0075m);
        public static readonly TransferTaxLine AlgonquinMunicipalTax 
            = new TransferTaxLine("Algonquin Municipal Tax", 0.0075m);

        public static readonly List<TransferTaxLine> DefaultTaxes = new List<TransferTaxLine>()
        {
            ILStateTax,
            MchenryCountyTax,
            AlgonquinMunicipalTax,
        };
    }
}