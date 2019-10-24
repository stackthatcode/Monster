namespace Monster.Web.Models.Sync
{
    public class OrderVerification
    {
        public long? ShopifyOrderId { get; set; }
        public string ShopifyOrderName { get; set; }
        public string ShopifyOrderCreatedAtUtc { get; set; }
        public string ShopifyOrderHref { get; set; }

        public bool IsValidOrder => ShopifyOrderId.HasValue;

        public static OrderVerification Empty()
        {
            return new OrderVerification();
        }
    }
}