using System;
using Monster.Acumatica.Api.Customer;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Transactions;

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

        public static bool ShouldCreatePayment(this UsrShopifyTransaction input)
        {
            return input.ShopifyGateway != Gateway.Manual
                   && input.UsrShopifyAcuPayment == null
                   && input.ShopifyStatus == TransactionStatus.Success
                   && (input.ShopifyKind == TransactionKind.Capture
                       || input.ShopifyKind == TransactionKind.Sale);
        }
    }
}
