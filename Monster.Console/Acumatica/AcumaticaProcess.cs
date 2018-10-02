using Monster.Middle;

namespace Monster.ConsoleApp.Acumatica
{
    public class AcumaticaProcess
    {
        public static void Execute()
        {
            using (var container = MiddleAutofac.Build())
            using (var scope = container.BeginLifetimeScope())
            {
                AcumaticaHarness.BeginSession(scope);
                AcumaticaHarness.RetrieveItemClass(scope);
                AcumaticaHarness.RetrieveCustomer(scope);
                //AcumaticaHarness.NewCustomerRecord(scope);
            }
        }
    }
}

