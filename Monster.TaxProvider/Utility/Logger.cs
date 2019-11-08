using PX.Data;

namespace Monster.TaxProvider.Utility
{
    public class Logger
    {
        private readonly bool _debugEnabled;
        public const string TaxProviderName = "Logic Automated -> Tax Provider";

        public Logger(bool debugEnabled)
        {
            _debugEnabled = debugEnabled;
        }

        public void Info(string message)
        {
            PXTrace.WriteInformation($"{TaxProviderName} - {message}");
        }

        public void Debug(string message)
        {
            if (_debugEnabled)
            {
                PXTrace.WriteInformation($"{TaxProviderName} - {message}");
            }
        }
    }
}
