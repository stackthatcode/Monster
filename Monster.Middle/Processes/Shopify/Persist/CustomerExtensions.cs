using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Customer;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public static class CustomerExtensions
    {
        public static Customer ToJsonObj(this ShopifyCustomer record)
        {
            return record.ShopifyJson.DeserializeFromJson<Push.Shopify.Api.Customer.Customer>();
        }
    }
}
