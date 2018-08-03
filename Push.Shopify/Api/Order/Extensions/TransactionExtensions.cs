namespace Push.Shopify.Api.Order.Extensions
{
    public static class TransactionExtensions
    {
        public static bool IsSuccess(this Transaction transaction)
        {
            return transaction.status == "success";
        }
    }
}
