using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Sync
{
    public static class CustomerExtensions
    {
        public static UsrAcumaticaCustomer
                MatchingCustomer(this UsrShopifyCustomer input)
        {
            return input
                .UsrShopAcuCustomerSyncs
                .FirstOrDefault()?
                .UsrAcumaticaCustomer;
        }
    }
}
