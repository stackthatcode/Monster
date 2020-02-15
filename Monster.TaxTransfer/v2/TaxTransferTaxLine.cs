namespace Monster.TaxTransfer.v2
{
    public class TaxTransferTaxLine
    {
        public string Title { get; set; }
        public decimal Rate { get; set; }

        public TaxTransferTaxLine(string title, decimal rate)
        {
            Title = title;
            Rate = rate;
        }
    }
}
