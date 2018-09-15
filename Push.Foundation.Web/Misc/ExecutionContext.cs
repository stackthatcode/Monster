using System;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Web.Misc
{
    public class ExecutionContext
    {
        public IPushLogger Logger { get; set; }
        public int NumberOfAttempts { get; set; }
        public string ThrottlingKey { get; set; }

        public ExecutionContext()
        {
            Logger = new ConsoleAndDebugLogger();
            NumberOfAttempts = 1;
            ThrottlingKey = Guid.NewGuid().ToString();
        }
    }
}
