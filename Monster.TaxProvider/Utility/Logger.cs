using PX.Data;

namespace Monster.TaxProvider.Utility
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
