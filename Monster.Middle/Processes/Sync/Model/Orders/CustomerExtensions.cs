using System;
using System.Linq;
using Monster.Acumatica.Api.Customer;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class CustomerExtensions
    {
        public static AcumaticaCustomer ToMonsterRecord(this Customer customer)
        {
            var output = new AcumaticaCustomer();
            output.AcumaticaCustomerId = customer.CustomerID.value;
            output.AcumaticaJson = customer.SerializeToJson();
            output.AcumaticaMainContactEmail = customer.MainContact.Email.value;
            output.DateCreated = DateTime.UtcNow;
            output.LastUpdated = DateTime.UtcNow;
            return output;
        }

        public static AcumaticaCustomer Match(this ShopifyCustomer input)
        {
            return input
                .ShopAcuCustomerSyncs
                .FirstOrDefault()?
                .AcumaticaCustomer;
        }

        public static bool HasMatch(this ShopifyCustomer input)
        {
            return input.Match() != null;
        }

        public static ShopAcuCustomerSync SyncRecord(this ShopifyCustomer input)
        {
            return input.ShopAcuCustomerSyncs.FirstOrDefault();
        }


        public static string AcumaticaCustId(this ShopifyCustomer input)
        {
            return input.HasMatch() ? input.Match().AcumaticaCustomerId : null;
        }
    }
}
