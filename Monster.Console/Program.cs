using System;
using Autofac;
using Monster.ConsoleApp.Acumatica;
using Monster.ConsoleApp.TestJson;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ExecuteInLifetimeScope(scope => MonsterHarness.PullPayoutsIntoAcumatica(scope));            
            
            Console.WriteLine("Finished - hit any key to exit...");
            Console.ReadKey();
        }


        static void DeserializeJson<T>(string inputJsonFile)
        {
            var json = TestLoader.GimmeJson(inputJsonFile);
            var deserializedObject = json.DeserializeFromJson<T>();
            var reserializedJson = deserializedObject.SerializeToJson();
            Console.WriteLine(reserializedJson);
        }
        
        static void ExecuteInLifetimeScope(Action<ILifetimeScope> action)
        {
            using (var container = ConsoleAutofac.Build(false))
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<IPushLogger>();
                try
                {
                    action(scope);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw;
                }
            }
        }
    }
}

