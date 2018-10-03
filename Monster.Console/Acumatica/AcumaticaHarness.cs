using Autofac;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Push.Foundation.Utilities.Json;

namespace Monster.ConsoleApp.Acumatica
{
    public class AcumaticaHarness
    {
        // Use configuration for Credentials
        public static void BeginSession(ILifetimeScope scope)
        {
            var credentials = AcumaticaCredentialsConfig.Settings.ToCredentials();

            // Load the Acumatica Context with our Credentials
            var acumaticaContext = scope.Resolve<AcumaticaHttpContext>();
            acumaticaContext.Initialize(credentials);

            // Begin our Session
            acumaticaContext.Begin();
        }


        // Acumatica 
        public static void RetrieveItemAndPostingClass(ILifetimeScope scope)
        {
            // Start doing things!
            var inventoryRepository = scope.Resolve<InventoryRepository>();

            var results = inventoryRepository.RetrieveItemClass();
            var results2 = inventoryRepository.RetrievePostingClasses();
        }
        
        public static void RetrieveCustomer(ILifetimeScope scope)
        {            
            // Start doing things...
            var customerRepository = scope.Resolve<CustomerRepository>();                
            var results = customerRepository.RetrieveCustomer("C000000001");

            var customer = results.DeserializeFromJson<Customer>();

            var backToJson = customer.SerializeToJson();
        }
        
        public static string NewCustomerRecord()
        {
            // Customer data
            string customerID = "JOHNGYXZ"; //Specify any customer ID that does not exist in the system
            string customerName = "Johnny Bad";
            string customerMainContactEmail = "demo@gmail.com";
            string customerMainContactAddressLine1 = "4030 Lake Washington Blvd NE";
            string customerMainContactAddressLine2 = "Suite 100";
            string customerMainContactCity = "Kirkland";
            string customerMainContactState = "WA";
            string customerMainContactPostalCode = "98033";

            //Convert the customer data to JSON format
            string entityAsString = new
            {
                CustomerID = new { value = customerID },
                CustomerName = new { value = customerName },
                MainContact = new
                {
                    Email = new { value = customerMainContactEmail },
                    Address = new
                    {
                        AddressLine1 = new { value = customerMainContactAddressLine1 },
                        AddressLine2 = new { value = customerMainContactAddressLine2 },
                        City = new { value = customerMainContactCity },
                        State = new { value = customerMainContactState },
                        PostalCode = new { value = customerMainContactPostalCode }
                    }
                }
            }.SerializeToJson();

            return entityAsString;
        }
        
    }
}
