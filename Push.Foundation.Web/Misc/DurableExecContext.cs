using System;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Web.Misc
{
    public class DurableExecContext
    {
        public IPushLogger Logger { get; set; }
        public int NumberOfAttempts { get; set; }
        public string ThrottlingKey { get; set; }

        public DurableExecContext()
        {
            Logger = new ConsoleAndDebugLogger();
            NumberOfAttempts = 1;
            ThrottlingKey = Guid.NewGuid().ToString();
        }
    }
}
