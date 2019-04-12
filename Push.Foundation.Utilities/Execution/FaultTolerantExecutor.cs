using System;
using Castle.Core.Internal;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Utilities.Execution
{
    public class FaultTolerantExecutor
    {
        public IPushLogger Logger { get; set; }
        public int MaxNumberOfAttempts { get; set; }
        public string ThrottlingKey { get; set; }
        public int ThrottlingDelay { get; set; }


        public FaultTolerantExecutor()
        {
            Logger = new ConsoleAndDebugLogger();
            MaxNumberOfAttempts = 1;
            ThrottlingKey = Guid.NewGuid().ToString();
            ThrottlingDelay = 0;
        }


        public T Do<T>(Func<T> task, string errorContext = null)
        {
            // Do Request and process the HTTP Status Codes
            var startTime = DateTime.UtcNow;

            // Execute task
            var result = DoWorker(task, errorContext);

            // Log execution time            
            var executionTime = DateTime.UtcNow - startTime;
            Logger.Debug($"Call performance - {executionTime} ms");
            
            return result;
        }


        private T DoWorker<T>(Func<T> task, string errorContext = null)
        {
            var numberOfAttempts = 1;

            while (true)
            {
                // Invoke the Throttler
                Throttler.Process(ThrottlingKey, ThrottlingDelay);

                try
                {
                    numberOfAttempts++;
                    return task();
                }
                catch (Exception ex)
                {                    
                    if (!errorContext.IsNullOrEmpty())
                    {
                        Logger.Error(ex, errorContext);
                    }
                    else
                    {
                        Logger.Error(ex);
                    }

                    if (numberOfAttempts >= MaxNumberOfAttempts)
                    {
                        Logger.Warn("Retry Limit has been exceeded... throwing exception");
                        throw;
                    }

                    Logger.Debug("Encountered an exception. Retrying invocation...");
                }
            }
        }
    }
}

