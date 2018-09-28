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
            builder.RegisterType<ShopifyHttpSettings>();

            builder.RegisterType<ShopifyHttpContext>();
                   
            builder.RegisterType<ShopApi>();
            builder.RegisterType<OrderApi>();
            builder.RegisterType<ProductApi>();
            builder.RegisterType<EventApi>();
            builder.RegisterType<PayoutApi>();
            builder.RegisterType<InventoryApi>();
        }
    }
}

