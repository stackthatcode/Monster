//using System;
//using Autofac;
//using Monster.Acumatica.Api;
//using Monster.Acumatica.Config;
//using Monster.Acumatica.Http;
//using Monster.Acumatica.Http.Cash;
//using Monster.Acumatica.Http.Common;
//using Monster.Acumatica.Http.Customer;
//using Push.Foundation.Utilities.Json;
//using Push.Foundation.Utilities.Logging;

//namespace Monster.ConsoleApp.Acumatica
//{
//    public class AcumaticaHarness
//    {
//        // Acumatica 
//        public static void RetrieveItemClass(ILifetimeScope scope)
//        {
//            // Pull these from secure storage
//            var config = AcumaticaSecurityConfig.Settings;
//            var credentials = new AcumaticaSecuritySettings(config);

//            // Spawn or constructor inject this factory
//            var factory = scope.Resolve<AcumaticaApiFactory>();

//            // Make repository - done!
//            var repository = factory.MakeSpikeRepository(credentials);

//            repository.RetrieveSession(credentials);
//            repository.RetrieveSession(credentials);
//            var results = repository.RetrieveItemClass();
//        }

//        public static void RetrievePostingClass(ILifetimeScope scope)
//        {
//            // Pull these from secure storage
//            var config = AcumaticaSecurityConfig.Settings;
//            var credentials = new AcumaticaSecuritySettings(config);

//            // Spawn or constructor inject this factory
//            var factory = scope.Resolve<AcumaticaApiFactory>();

//            // Make repository - done!
//            var repository = factory.MakeSpikeRepository(credentials);

//            repository.RetrieveSession(credentials);
//            var results = repository.RetrievePostingClasses();
//        }

//        public static void RetrieveAndAddNewCustomer(ILifetimeScope scope)
//        {
//            // Pull these from secure storage
//            var config = AcumaticaSecurityConfig.Settings;
//            var credentials = new AcumaticaSecuritySettings(config);

//            // Spawn or constructor inject this factory
//            var factory = scope.Resolve<AcumaticaApiFactory>();

//            // Create the repository passing the credentials and invoke!
//            var repository = factory.MakeSpikeRepository(credentials);
//            repository.RetrieveSession(credentials);

//            //var results = repository.RetrieveCustomers();
//            //var customers = results.DeserializeFromJson<Customer[]>();

//            var results = repository.RetrieveCustomer("C000000001");

//            var customer = results.DeserializeFromJson<Customer>();

//            var backToJson = customer.SerializeToJson();
//        }

//        public static void AddNewCustomer(ILifetimeScope scope)
//        {
//            // Pull these from secure storage
//            var config = AcumaticaSecurityConfig.Settings;
//            var credentials = new AcumaticaSecuritySettings(config);

//            // Spawn or constructor inject this factory
//            var factory = scope.Resolve<AcumaticaApiFactory>();

//            // Make repository - done!
//            var repository = factory.MakeSpikeRepository(credentials);

//            repository.RetrieveSession(credentials);

//            var customers = repository.RetrieveCustomers();

//            //var newCustomer = new Customer
//            //{
//            //    CustomerName = "Test Customer ABC".ToValue(),
//            //};

//            repository.AddNewCustomer(NewCustomerRecord());
//        }

//        public static string NewCustomerRecord()
//        {
//            // Customer data
//            string customerID = "JOHNGYXZ"; //Specify any customer ID that does not exist in the system
//            string customerName = "Johnny Bad";
//            string customerMainContactEmail = "demo@gmail.com";
//            string customerMainContactAddressLine1 = "4030 Lake Washington Blvd NE";
//            string customerMainContactAddressLine2 = "Suite 100";
//            string customerMainContactCity = "Kirkland";
//            string customerMainContactState = "WA";
//            string customerMainContactPostalCode = "98033";

//            //Convert the customer data to JSON format
//            string entityAsString = new
//            {
//                CustomerID = new { value = customerID },
//                CustomerName = new { value = customerName },
//                MainContact = new
//                {
//                    Email = new { value = customerMainContactEmail },
//                    Address = new
//                    {
//                        AddressLine1 = new { value = customerMainContactAddressLine1 },
//                        AddressLine2 = new { value = customerMainContactAddressLine2 },
//                        City = new { value = customerMainContactCity },
//                        State = new { value = customerMainContactState },
//                        PostalCode = new { value = customerMainContactPostalCode }
//                    }
//                }
//            }.SerializeToJson();

//            return entityAsString;
//        }


//        public static void RetrieveImportBankTransactions(ILifetimeScope scope)
//        {
//            // Object instancing
//            var factory = scope.Resolve<AcumaticaApiFactory>();
//            var logger = scope.Resolve<IPushLogger>();
            
//            // Pull these from secure storage
//            var config = AcumaticaSecurityConfig.Settings;
//            var credentials = new AcumaticaSecuritySettings(config);

//            // Create the repository passing the credentials and create session
//            var repository = factory.MakeSpikeRepository(credentials);
//            repository.RetrieveSession(credentials);

//            // Get the Bank Transactions
//            var results = repository.RetrieveImportBankTransactions();
//            logger.Info(results);
//        }

//        public static void InsertImportBankTransactions(ILifetimeScope scope)
//        {
//            // Object instancing
//            var factory = scope.Resolve<AcumaticaApiFactory>();
//            var logger = scope.Resolve<IPushLogger>();

//            // Pull these from secure storage
//            var config = AcumaticaSecurityConfig.Settings;
//            var credentials = new AcumaticaSecuritySettings(config);

//            // Create the repository passing the credentials and create session
//            var repository = factory.MakeSpikeRepository(credentials);
//            repository.RetrieveSession(credentials);

//            // Get the Bank Transactions
//            // TODO - create simple UTC wrapper
//            var testDate = new DateTimeOffset(2018, 09, 14, 0, 0, 0, TimeSpan.Zero);

//            var transaction = new ImportBankTransaction
//            {
//                CashAccount =  "102000".ToValue(),
//                StatementDate = testDate.ToValue(),
//                StartBalanceDate = testDate.ToValue(),
//                EndBalanceDate = testDate.ToValue(),

//                ExtTranID = "JONES-1111-3333".ToValue(),
//                ExtRefNbr = "JONES-1111-3333".ToValue(),
//                Receipt = (100.0).ToValue(),
//                InvoiceNbr = "#5237710".ToValue(),
//            };

//            var results = repository.InsertImportBankTransaction(transaction.SerializeToJson());
//            logger.Info(results);
//        }
//    }
//}
