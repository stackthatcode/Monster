using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Utilities.Execution
{
    public class Throttler
    {                
        private static readonly 
            IDictionary<string, DateTime> 
                LastExecutionTime = new ConcurrentDictionary<string, DateTime>();
        
        public static void Process(
                string key, 
                int timeBetweenCallsMs = 0,
                IPushLogger logger = null)
        {
            if (logger == null)
            {
                logger = new ConsoleAndDebugLogger();
            }

            if (LastExecutionTime.ContainsKey(key))
            {
                var lastExecutionTime = LastExecutionTime[key];

                var timeSinceLastExecution = DateTime.UtcNow - lastExecutionTime;
                var throttlingDelay = new TimeSpan(0, 0, 0, 0, timeBetweenCallsMs);

                if (timeSinceLastExecution < throttlingDelay)
                {
                    var remainingTimeToDelay = throttlingDelay - timeSinceLastExecution;

                    logger.Debug($"Intentional delay before next call: {remainingTimeToDelay} ms");

                    Thread.Sleep(remainingTimeToDelay);
                }
            }
            
            LastExecutionTime[key] = DateTime.UtcNow;
        }
    }
}
