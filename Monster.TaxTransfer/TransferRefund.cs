namespace Monster.TaxTransfer
{
    public class TransferRefund
    {
        public string ExternalRefNbr { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal NonFreightTax { get; set; }
        public decimal FreightTax { get; set; }
    }
}

