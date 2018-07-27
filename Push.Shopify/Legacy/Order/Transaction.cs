namespace Push.Shopify.Legacy.Order
{
    public class Transaction
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }

        public bool IsSuccess => Status == "success";
    }
}
