using System.Linq;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Processes.Sync.Extensions
{
    public static class CustomerExtensions
    {
        public static 
                UsrAcumaticaCustomer Match(this UsrShopifyCustomer input)
        {
            return input
                .UsrShopAcuCustomerSyncs
                .FirstOrDefault()?
                .UsrAcumaticaCustomer;
        }

        public static bool HasMatch(this UsrShopifyCustomer input)
        {
            return input.Match() != null;
        }

        public static string AcumaticaCustId(this UsrShopifyCustomer input)
        {
            return input.HasMatch() ? input.Match().AcumaticaCustomerId : null;
        }
    }
}
