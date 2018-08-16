using Autofac;
using Push.Shopify.Api;
using Push.Shopify.Config;
using Push.Shopify.HttpClient;


namespace Push.Shopify
{
    public class ShopifyApiAutofac
    {
        public static void Build(ContainerBuilder builder)
        {            
            builder.RegisterType<ShopifyRequestBuilder>();
            builder.RegisterType<ShopifyClientSettings>();

            builder.RegisterType<ApiFactory>();     
                   
            builder.RegisterType<ShopApiRepository>();
            builder.RegisterType<OrderApiRepository>();
            builder.RegisterType<ProductApiRepository>();
            builder.RegisterType<EventApiRepository>();
            builder.RegisterType<PayoutApiRepository>();
        }        
    }
}

