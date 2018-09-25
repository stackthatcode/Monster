using Autofac;
using Push.Shopify.Api;
using Push.Shopify.Config;
using Push.Shopify.Http;


namespace Push.Shopify
{
    public class ShopifyApiAutofac
    {
        public static void Build(ContainerBuilder builder)
        {            
            builder.RegisterType<ShopifyHttpClientFactory>();
            builder.RegisterType<ShopifyClientSettings>();

            builder.RegisterType<ShopifyApiFactory>();     
                   
            builder.RegisterType<ShopRepository>();
            builder.RegisterType<OrderRepository>();
            builder.RegisterType<ProductRepository>();
            builder.RegisterType<EventRepository>();
            builder.RegisterType<PayoutRepository>();
            builder.RegisterType<InventoryRepository>();
        }
    }
}

