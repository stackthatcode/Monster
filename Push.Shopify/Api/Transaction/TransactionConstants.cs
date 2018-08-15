namespace Push.Shopify.Api.Transaction
{
    public class TransactionStatus
    {
        public const string Pending = "pending";
        public const string Success = "success";
        public const string Failure = "failure";
        public const string Error = "error";
    }

    public class TransactionKind
    {
        public const string Authorization = "authorization";
        public const string Capture = "capture";
        public const string Sale = "sale";
        public const string Void = "void";
        public const string Refund = "refund";
    }
}
