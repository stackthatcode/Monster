using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Web.Misc
{
    public class Throttler
    {        
        public int TimeBetweenCallsMs;
        private readonly IPushLogger _logger;

        private static readonly 
            IDictionary<string, DateTime> 
                LastExecutionTime = new ConcurrentDictionary<string, DateTime>();

        

        public Throttler(IPushLogger logger)
        {
            _logger = logger;
        }

        public Throttler()
        {
            _logger = new ConsoleAndDebugLogger();
        }


        public virtual void Process(string key)
        {
            if (LastExecutionTime.ContainsKey(key))
            {
                var lastExecutionTime = LastExecutionTime[key];

                var timeSinceLastExecution = DateTime.UtcNow - lastExecutionTime;
                var throttlingDelay = new TimeSpan(0, 0, 0, 0, TimeBetweenCallsMs);

                if (timeSinceLastExecution < throttlingDelay)
                {
                    var remainingTimeToDelay = throttlingDelay - timeSinceLastExecution;

                    _logger.Debug($"Intentional delay before next call: {remainingTimeToDelay} ms");

                    Thread.Sleep(remainingTimeToDelay);
                }
            }
            
            LastExecutionTime[key] = DateTime.UtcNow;
        }
    }
}
