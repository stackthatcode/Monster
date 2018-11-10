namespace Monster.Middle.Persist.Multitenant.Shopify
{
    // Mirrors Shopify's Order -> Financial Status field
    public class FinancialStatus
    {
        public const string Pending ="pending";
        public const string Authorized = "authorized";
        public const string PartiallyPaid = "partially_paid";
        public const string Paid = "paid";
        public const string PartiallyRefunded = "partially_refunded";
        public const string Refunded = "refunded";
        public const string Voided = "voided";
    }

}
