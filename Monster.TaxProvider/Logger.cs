using PX.Data;

namespace Monster.TaxProvider
{
    public class Logger
    {
        public const string TaxProviderName = "Logic Automated -> Tax Provider";

        public void Info(string message)
        {
            PXTrace.WriteInformation($"{TaxProviderName} - {message}");
        }
    }
}
