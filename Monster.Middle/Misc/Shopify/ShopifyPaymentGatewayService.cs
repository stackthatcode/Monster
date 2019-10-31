using System.Collections.Generic;

namespace Monster.Middle.Misc.Shopify
{
    public class ShopifyPaymentGatewayService
    {
        public List<ShopifyPaymentGateway> Retrieve()
        {
            return new List<ShopifyPaymentGateway>
            {
                new ShopifyPaymentGateway
                {
                    Id = "shopify_payments",
                    Name = "Shopify Payments",
                },
                new ShopifyPaymentGateway
                {
                    Id = "paypal",
                    Name = "PayPal",
                },  
                new ShopifyPaymentGateway
                {
                    Id = "amazon_payments",
                    Name = "Amazon",
                },
            };
        }
    }
}
