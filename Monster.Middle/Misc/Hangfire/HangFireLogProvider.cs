using Hangfire.Logging;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Misc.Hangfire
{
    public class HangfireLogProvider : ILogProvider
    {
        private static IPushLogger _loggerInstance;

        public static void RegisterInstance(IPushLogger loggerInstance)
        {
            _loggerInstance = loggerInstance;
        }

        public ILog GetLogger(string name)
        {
            return new HangfireLogger(_loggerInstance);
        }
    }
}
