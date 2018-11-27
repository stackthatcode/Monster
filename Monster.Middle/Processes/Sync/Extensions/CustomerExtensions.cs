using System.Linq;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Processes.Sync.Extensions
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
