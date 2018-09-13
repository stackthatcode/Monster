using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Monster.Acumatica.Model;
using Push.Foundation.Utilities.Json;

namespace Monster.ConsoleApp
{
    public class AcumaticaTestbed
    {
        // Acumatica 
        public void RetrieveAcumaticaItemClass(ILifetimeScope scope)
        {
            // Pull these from secure storage
            var config = AcumaticaSecurityConfig.Settings;
            var credentials = new AcumaticaSecuritySettings(config);

            // Spawn or constructor inject this factory
            var factory = scope.Resolve<AcumaticaApiFactory>();

            // Make repository - done!
            var repository = factory.MakeSpikeRepository(credentials);

            repository.RetrieveSession(credentials);
            repository.RetrieveSession(credentials);
            var results = repository.RetrieveItemClass();
        }

        public void RetrieveAcumaticaPostingClass(ILifetimeScope scope)
        {
            // Pull these from secure storage
            var config = AcumaticaSecurityConfig.Settings;
            var credentials = new AcumaticaSecuritySettings(config);

            // Spawn or constructor inject this factory
            var factory = scope.Resolve<AcumaticaApiFactory>();

            // Make repository - done!
            var repository = factory.MakeSpikeRepository(credentials);

            repository.RetrieveSession(credentials);
            var results = repository.RetrievePostingClasses();
        }

        public void RetrieveAcumaticaCustomer(ILifetimeScope scope)
        {
            // Pull these from secure storage
            var config = AcumaticaSecurityConfig.Settings;
            var credentials = new AcumaticaSecuritySettings(config);

            // Spawn or constructor inject this factory
            var factory = scope.Resolve<AcumaticaApiFactory>();

            // Make repository - done!
            var repository = factory.MakeSpikeRepository(credentials);

            repository.RetrieveSession(credentials);

            //var results = repository.RetrieveCustomers();
            //var customers = results.DeserializeFromJson<Customer[]>();

            var results = repository.RetrieveCustomer("C000000001");

            var customer = results.DeserializeFromJson<Customer>();

            var backToJson = customer.SerializeToJson();
        }

        public void AddNewAcumaticaCustomer(ILifetimeScope scope)
        {
            // Pull these from secure storage
            var config = AcumaticaSecurityConfig.Settings;
            var credentials = new AcumaticaSecuritySettings(config);

            // Spawn or constructor inject this factory
            var factory = scope.Resolve<AcumaticaApiFactory>();

            // Make repository - done!
            var repository = factory.MakeSpikeRepository(credentials);

            repository.RetrieveSession(credentials);


            //var customer = new Customer
            //{
            //    Address = 
            //}
        }

    }
}
