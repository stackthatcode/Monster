using System;
using System.Linq;
using Monster.Acumatica.Api.Customer;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Sync.Extensions
{
    public static class CustomerExtensions
    {
        public static UsrAcumaticaCustomer ToMonsterRecord(this Customer customer)
        {
            var output = new UsrAcumaticaCustomer();
            output.AcumaticaCustomerId = customer.CustomerID.value;
            output.AcumaticaJson = customer.SerializeToJson();
            output.AcumaticaMainContactEmail = customer.MainContact.Email.value;
            output.DateCreated = DateTime.UtcNow;
            output.LastUpdated = DateTime.UtcNow;
            return output;
        }

        public static UsrAcumaticaCustomer Match(this UsrShopifyCustomer input)
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
