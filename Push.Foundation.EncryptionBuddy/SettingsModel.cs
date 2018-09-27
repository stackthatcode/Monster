using Newtonsoft.Json;
using Push.Shopify.Config;

namespace Push.Foundation
{
    public class RootObject
    {
        public ShopifyCredentials Configuration { get; set; }

        public static RootObject FromJson(string json) => JsonConvert.DeserializeObject<RootObject>(json);
    }

}

