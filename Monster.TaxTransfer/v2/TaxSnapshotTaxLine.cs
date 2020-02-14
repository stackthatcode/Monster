namespace Monster.TaxTransfer.v2
{
    public class TaxSnapshotTaxLine
    {
        public string Title { get; set; }
        public decimal Rate { get; set; }

        public TaxSnapshotTaxLine(string title, decimal rate)
        {
            Title = title;
            Rate = rate;
        }
    }
}
