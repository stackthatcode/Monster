using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.Misc.Shopify
{
    public class ShopifyPaymentGatewayService
    {
        private readonly static List<ShopifyPaymentGateway> _data 
        = new List<ShopifyPaymentGateway>
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
            new ShopifyPaymentGateway
            {
                Id = "bogus",
                Name = "Bogus Gateway (TEST)",
            },
        };

        public List<ShopifyPaymentGateway> Retrieve()
        {
            return _data;
        }

        public ShopifyPaymentGateway Retrieve(string gatewayId)
        {
            return _data.FirstOrDefault(x => x.Id == gatewayId);
        }
        

        public string Name(string gatewayId)
        {
            var gateway = Retrieve().FirstOrDefault(x => x.Id == gatewayId);
            return gateway == null ? "(invalid gateway)" : gateway.Name;
        }
    }
}
