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
            builder.RegisterType<ShopifyHttpConfig>();
            builder.RegisterType<ShopifyCredentialsConfig>();

            builder.RegisterType<ShopifyHttpContext>().InstancePerLifetimeScope(); 

            builder.RegisterType<ShopApi>().InstancePerLifetimeScope();
            builder.RegisterType<OrderApi>().InstancePerLifetimeScope();
            builder.RegisterType<ProductApi>().InstancePerLifetimeScope();
            builder.RegisterType<EventApi>().InstancePerLifetimeScope();
            builder.RegisterType<PayoutApi>().InstancePerLifetimeScope();
            builder.RegisterType<InventoryApi>().InstancePerLifetimeScope();
        }
    }
}

