namespace Monster.TaxTransfer
{
    public class TransferRefund
    {
        public string ExternalSystemRefNbr { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal NonFreightTax { get; set; }
        public decimal FreightTax { get; set; }
    }
}

