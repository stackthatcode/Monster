namespace Monster.TaxTransfer
{
    public class TransferRefund
    {
        public string ExternalRefNbr { get; set; }
        
        public decimal LineItemTotal { get; set; }
        public decimal LineItemsTax { get; set; }
        public decimal Freight { get; set; }
        public decimal FreightTax { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxTotal => LineItemsTax + FreightTax;

        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal NetCreditDebit => Credit - Debit;
        
        public decimal RefundAmount { get; set; }

        public decimal ExpectedTotal
            => LineItemTotal + Freight + LineItemsTax + FreightTax + Credit - Debit;
        public decimal Overpayment => RefundAmount - ExpectedTotal;
    }
}

