using System;
using Monster.Acumatica.Api.Customer;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Sync.Orders.Model
{
    public static class Extensions
    {
        public static 
                UsrAcumaticaCustomer ToMonsterRecord(this Customer customer)
        {
            var output = new UsrAcumaticaCustomer();
            output.AcumaticaCustomerId = customer.CustomerID.value;
            output.AcumaticaJson = customer.SerializeToJson();
            output.AcumaticaMainContactEmail = customer.MainContact.Email.value;
            output.DateCreated = DateTime.UtcNow;
            output.LastUpdated = DateTime.UtcNow;
            return output;
        }
    }
}
